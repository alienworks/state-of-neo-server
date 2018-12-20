using Akka.Actor;
using Microsoft.EntityFrameworkCore;
using Neo.Ledger;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Neo.Ledger.Blockchain;

namespace StateOfNeo.Server.Actors
{
    public class NodePersister : UntypedActor
    {
        private const string LatencyCacheType = "latency";
        private const string PeersCacheType = "peers";

        private readonly string connectionString;
        private readonly string net;
        private readonly RPCNodeCaller nodeCaller;

        private Dictionary<string, Dictionary<int, ICollection<long>>> NodesAuditCache { get; set; }

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
                if (m.Block.Index % 300 == 0)
                {
                    var optionsBuilder = new DbContextOptionsBuilder<StateOfNeoContext>();
                    optionsBuilder.UseSqlServer(this.connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
                    var db = new StateOfNeoContext(optionsBuilder.Options);

                    var previousBlock = Blockchain.Singleton.GetBlock(m.Block.PrevHash);
                    var previousBlockTime = previousBlock.Timestamp.ToUnixDate();

                    var nodes = db.Nodes
                        .Include(x => x.Audits)
                        .Where(x => x.Net == this.net && x.SuccessUrl != null)
                        .ToList();

                    foreach (var node in nodes)
                    {
                        var audit = this.NodeAudit(node, m.Block.Timestamp, (m.Block.Timestamp.ToUnixDate() - previousBlockTime).TotalSeconds);

                        if (audit != null)
                        {
                            db.NodeAudits.Add(audit);
                            node.LastAudit = m.Block.Timestamp;
                        }

                        db.Nodes.Update(node);
                        db.SaveChanges();
                    }
                }
            }
        }

        private void UpdateNodeTimes(Node node, uint timestamp, double secondsOnline)
        {
            if (!node.FirstRuntime.HasValue)
            {
                node.FirstRuntime = timestamp;
            }

            if (!node.LatestRuntime.HasValue)
            {
                node.LatestRuntime = node.FirstRuntime;
                node.SecondsOnline = 0;
            }
            else
            {
                node.SecondsOnline += (long)secondsOnline;
                node.LatestRuntime = timestamp;
            }
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

        private double GetAuditValue(string auditType, int nodeId)
        {
            var result = this.NodesAuditCache[auditType][nodeId].Average();
            this.NodesAuditCache[auditType][nodeId].Clear();

            return result;
        }

        private NodeAudit NodeAudit(Node node, uint blockStamp, double secondsSinceLastBlock)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var height = this.nodeCaller.GetNodeHeight(node).GetAwaiter().GetResult();
            stopwatch.Stop();

            if (height.HasValue)
            {
                node.Height = height.Value;
                this.UpdateNodeTimes(node, blockStamp, secondsSinceLastBlock);
                this.UpdateCache(LatencyCacheType, node.Id, stopwatch.ElapsedMilliseconds);

                var peers = this.nodeCaller.GetNodePeers(node).GetAwaiter().GetResult();
                if (peers != null)
                {
                    node.Peers = peers.Connected.Count();
                    this.UpdateCache(PeersCacheType, node.Id, peers.Connected.Count());
                }

                if (node.LastAudit == null || node.LastAudit.Value.ToUnixDate().AddHours(1) < blockStamp.ToUnixDate())
                {
                    var audit = new NodeAudit
                    {
                        NodeId = node.Id,
                        Peers = (decimal)this.GetAuditValue(PeersCacheType, node.Id),
                        Latency = (int)this.GetAuditValue(LatencyCacheType, node.Id),
                        CreatedOn = DateTime.UtcNow,
                        Timestamp = blockStamp
                    };

                    node.LastAudit = blockStamp;
                    return audit;
                }
            }

            return null;
        }
    }
}
