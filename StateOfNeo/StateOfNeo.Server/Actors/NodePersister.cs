using Akka.Actor;
using Microsoft.EntityFrameworkCore;
using Neo.Ledger;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Http;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using static Neo.Ledger.Blockchain;

namespace StateOfNeo.Server.Actors
{
    public class NodePersister : UntypedActor
    {
        private const string LatencyCacheType = "latency";
        private const string PeersCacheType = "peers";
        private const int MinutesPerAudit = 15;

        private readonly string connectionString;
        private readonly string net;
        private readonly RPCNodeCaller nodeCaller;

        private Dictionary<string, Dictionary<int, ICollection<long>>> NodesAuditCache { get; set; }
        private long? lastUpdateStamp = null;
        private long? totalSecondsElapsed = null;

        public NodePersister(IActorRef blockchain, string connectionString, string net, RPCNodeCaller nodeCaller)
        {
            this.connectionString = connectionString;
            this.net = net;
            this.nodeCaller = nodeCaller;
            this.NodesAuditCache = new Dictionary<string, Dictionary<int, ICollection<long>>>();

            blockchain.Tell(new Register());
        }

        public static Props Props(IActorRef blockchain, string connectionString, string net, RPCNodeCaller nodeCaller) =>
            Akka.Actor.Props.Create(() => new NodePersister(blockchain, connectionString, net, nodeCaller));

        protected override void OnReceive(object message)
        {
            if (message is PersistCompleted m)
            {
                if (this.lastUpdateStamp == null ||
                    this.lastUpdateStamp.Value.ToUnixDate().AddMinutes(MinutesPerAudit) <= m.Block.Timestamp.ToUnixDate())
                {
                    if (this.lastUpdateStamp == null) this.lastUpdateStamp = m.Block.Timestamp;

                    var optionsBuilder = new DbContextOptionsBuilder<StateOfNeoContext>();
                    optionsBuilder.UseSqlServer(this.connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
                    var db = new StateOfNeoContext(optionsBuilder.Options);

                    var previousBlock = Blockchain.Singleton.GetBlock(m.Block.PrevHash);
                    var previousBlockTime = previousBlock.Timestamp.ToUnixDate();

                    var nodes = db.Nodes
                        .Include(x => x.NodeAddresses)
                        .Include(x => x.Audits)
                        .Where(x => x.Net == this.net && x.SuccessUrl != null)
                        .ToList();

                    foreach (var node in nodes)
                    {
                        var audit = this.NodeAudit(node, m.Block);

                        if (audit != null)
                        {
                            db.NodeAudits.Add(audit);
                            node.LastAudit = m.Block.Timestamp;
                        }

                        db.Nodes.Update(node);
                        db.SaveChanges();
                    }

                    this.lastUpdateStamp = m.Block.Timestamp;
                    this.totalSecondsElapsed = null;
                }
            }
        }

        private void UpdateNodeTimes(Node node, Neo.Network.P2P.Payloads.Block block)
        {
            if (!node.FirstRuntime.HasValue)
            {
                node.FirstRuntime = block.Timestamp;
            }

            if (!node.LatestRuntime.HasValue)
            {
                node.LatestRuntime = node.FirstRuntime;
                node.SecondsOnline = 0;
            }
            else
            {
                node.SecondsOnline += this.GetTotalSecondsElapsed(block.Timestamp);
                node.LatestRuntime = block.Timestamp;
            }
        }

        private long GetTotalSecondsElapsed(long blockStamp)
        {
            if (this.totalSecondsElapsed == null)
            {
                this.totalSecondsElapsed = (long)(blockStamp.ToUnixDate() - this.lastUpdateStamp.Value.ToUnixDate()).TotalSeconds;
            }

            return this.totalSecondsElapsed.Value;
        }

        private void UpdateCache(string auditType, int nodeId, long latency)
        {
            if (!this.NodesAuditCache.ContainsKey(auditType))
            {
                this.NodesAuditCache.Add(auditType, new Dictionary<int, ICollection<long>>());
            }

            if (!this.NodesAuditCache[auditType].ContainsKey(nodeId))
            {
                this.NodesAuditCache[auditType].Add(nodeId, new List<long> { latency });
            }
            else
            {
                this.NodesAuditCache[auditType][nodeId].Add(latency);
            }
        }

        private double? GetAuditValue(string auditType, int nodeId)
        {
            double? result = null;

            if (this.NodesAuditCache.ContainsKey(auditType) && this.NodesAuditCache[auditType].ContainsKey(nodeId))
            {
                result = this.NodesAuditCache[auditType][nodeId].Average();
                this.NodesAuditCache[auditType][nodeId].Clear();
            }

            return result;
        }

        private NodeAudit NodeAudit(Node node, Neo.Network.P2P.Payloads.Block block)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int? height = null;
            if (node.Type == StateOfNeo.Common.NodeAddressType.REST)
            {
                if (node.Service == NodeCallsConstants.NeoScan)
                {
                    var heightResponse = HttpRequester.MakeRestCall<HeightResponseObject>($@"{node.Url}get_height", HttpMethod.Get)
                        .GetAwaiter()
                        .GetResult();

                    if (heightResponse != null)
                    {
                        height = heightResponse.Height;
                    }
                }
                else if (node.Service == NodeCallsConstants.NeoNotification)
                {
                    var versionResponse = HttpRequester.MakeRestCall<NeoNotificationVersionResponse>($@"{node.Url}version", HttpMethod.Get)
                        .GetAwaiter()
                        .GetResult();

                    if (versionResponse != null)
                    {
                        height = versionResponse.Height;
                    }
                }
            }
            else
            {
                height = this.nodeCaller.GetNodeHeight(node).GetAwaiter().GetResult();
            }

            if (height.HasValue)
            {
                node.Height = height.Value;
                this.UpdateNodeTimes(node, block);
                this.UpdateCache(LatencyCacheType, node.Id, stopwatch.ElapsedMilliseconds);

                if (node.Type == StateOfNeo.Common.NodeAddressType.RPC)
                {
                    var peers = this.nodeCaller.GetNodePeers(node).GetAwaiter().GetResult();
                    if (peers != null)
                    {
                        this.UpdateCache(PeersCacheType, node.Id, peers.Connected.Count());
                    }
                }

                if (node.LastAudit == null || node.LastAudit.Value.ToUnixDate().AddHours(1) < block.Timestamp.ToUnixDate())
                {
                    decimal? peers = null;
                    var peersValue = this.GetAuditValue(PeersCacheType, node.Id);
                    if (peersValue != null)
                    {
                        peers = (decimal)peersValue.Value;
                    }

                    var audit = new NodeAudit
                    {
                        NodeId = node.Id,
                        Peers = peers,
                        Latency = (int)this.GetAuditValue(LatencyCacheType, node.Id),
                        CreatedOn = DateTime.UtcNow,
                        Timestamp = block.Timestamp
                    };

                    node.LastAudit = block.Timestamp;
                    return audit;
                }
            }

            return null;
        }
    }
}
