using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Http;
using StateOfNeo.Common.RPC;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Infrastructure.RPC;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Neo.Ledger.Blockchain;

namespace StateOfNeo.Server.Actors
{
    public class NodeFinder : UntypedActor
    {
        private readonly string connectionString;
        private readonly NetSettings netSettings;
        private readonly IHubContext<PeersHub> peersHub;
        private readonly NodeCache nodeCache;

        private long? lastUpdateStamp = null;
        private long currentStamp;

        public NodeFinder(
            IActorRef blockchain,
            string connectionString,
            NetSettings netSettings,
            IHubContext<PeersHub> peersHub,
            NodeCache nodeCache)
        {
            this.connectionString = connectionString;
            this.netSettings = netSettings;
            this.peersHub = peersHub;
            this.nodeCache = nodeCache;

            blockchain.Tell(new Register());
        }

        public static Props Props(
            IActorRef blockchain,
            string connectionString,
            NetSettings netSettings,
            IHubContext<PeersHub> peersHub,
            NodeCache nodeCache) =>
                Akka.Actor.Props.Create(() =>
                    new NodeFinder(
                        blockchain,
                        connectionString,
                        netSettings,
                        peersHub,
                        nodeCache));

        protected override void OnReceive(object message)
        {
            if (message is PersistCompleted m)
            {
                if (this.lastUpdateStamp == null ||
                    this.lastUpdateStamp.Value.ToUnixDate().AddHours(24) <= m.Block.Timestamp.ToUnixDate())
                {
                    if (this.nodeCache.PeersCollected.Count > 0)
                    {
                        this.currentStamp = m.Block.Timestamp;
                        this.DoWork();
                    }
                }
            }
        }

        private void DoWork()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StateOfNeoContext>();
            optionsBuilder.UseSqlServer(this.connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
            var db = new StateOfNeoContext(optionsBuilder.Options);

            var dbNodeAddresses = db.NodeAddresses.Select(x => x.Ip.Trim()).ToList();
            var addresesesToCheck = this.nodeCache.PeersCollected
                .Where(x => !dbNodeAddresses.Contains(x.Address.Trim()))
                .ToList();

            var sw = Stopwatch.StartNew();
            var sessionId = Guid.NewGuid();
            Log.Information($"PEERS CHECK Start: {DateTime.UtcNow}. SessionId: {sessionId}");

            foreach (var address in addresesesToCheck)
            {
                this.HandleNewPeerIp(address.Address.ToMatchedIp(), db);
                this.HandleNewAddress(address, db);
            }

            sw.Stop();
            Log.Information($"PEERS CHECK End: {sw.ElapsedMilliseconds}. SessionId: {sessionId}");

            if (this.lastUpdateStamp == null) this.lastUpdateStamp = this.currentStamp;

            db.Dispose();
        }

        private void HandleNewPeerIp(string address, StateOfNeoContext db)
        {
            var existingAddress = db.NodeAddresses
                .Include(x => x.Node)
                .FirstOrDefault(x => x.Ip.ToMatchedIp() == address.ToMatchedIp());

            if (existingAddress != null)
            {
                var newPeer = new Peer
                {
                    Ip = address,
                    FlagUrl = existingAddress.Node.FlagUrl,
                    Locale = existingAddress.Node.Locale,
                    Latitude = existingAddress.Node.Latitude,
                    Longitude = existingAddress.Node.Longitude,
                    Location = existingAddress.Node.Location,
                    NodeId = existingAddress.NodeId
                };

                this.nodeCache.AddPeerToCache(newPeer);
                var peerModel = AutoMapper.Mapper.Map<PeerViewModel>(newPeer);
                this.peersHub.Clients.All.SendAsync("new", peerModel);
            }
            else if (!db.Peers.Any(x => x.Ip == address))
            {
                var location = LocationCaller.GetIpLocation(address).GetAwaiter().GetResult();

                if (location != null)
                {
                    var newPeer = new Peer
                    {
                        Ip = address,
                        FlagUrl = location.Location.Flag,
                        Locale = location.Location.Languages?.FirstOrDefault().Code,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Location = location.CountryName
                    };

                    db.Peers.Add(newPeer);
                    db.SaveChanges();

                    this.nodeCache.AddPeerToCache(newPeer);
                    var peerModel = AutoMapper.Mapper.Map<PeerViewModel>(newPeer);
                    this.peersHub.Clients.All.SendAsync("new", peerModel);
                }
            }
        }

        private void HandleNewAddress(RPCPeer address, StateOfNeoContext db)
        {
            var newNode = default(Node);
            string successUrl = null;
            var ports = this.netSettings.GetPorts();

            foreach (var portWithType in ports)
            {
                var url = portWithType.GetFullUrl(address.Address.ToMatchedIp());

                try
                {
                    var rpcResult = RpcCaller.MakeRPCCall<RPCResponseBody<int>>(url, "getblockcount")
                        .GetAwaiter()
                        .GetResult();


                    if (rpcResult?.Result > 0)
                    {
                        successUrl = url;
                        newNode = this.CreateNodeOfAddress(address.Address,
                            portWithType.Type,
                            successUrl,
                            NodeAddressType.RPC);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Get blockcount parse error {e.Message}", e);
                    break;
                }
            }

            if (newNode == default(Node))
            {
                var httpTypes = new string[] { "https", "http" };
                foreach (var httpType in httpTypes)
                {
                    var url = $"{httpType}://{address.Address.ToMatchedIp()}";
                    var heightResponse = HttpRequester.MakeRestCall<HeightResponseObject>($@"{url}/api/main_net/v1/get_height", HttpMethod.Get)
                            .GetAwaiter()
                            .GetResult();

                    if (heightResponse != null)
                    {
                        successUrl = url;
                        newNode = this.CreateNodeOfAddress(address.Address,
                            httpType,
                            successUrl,
                            NodeAddressType.REST,
                            NodeCallsConstants.NeoScan);
                        break;
                    }

                    var versionResponse = HttpRequester.MakeRestCall<NeoNotificationVersionResponse>($@"{url}/v1/version", HttpMethod.Get)
                            .GetAwaiter()
                            .GetResult();

                    if (versionResponse != null)
                    {
                        successUrl = url;
                        newNode = this.CreateNodeOfAddress(address.Address,
                            httpType,
                            successUrl,
                            NodeAddressType.REST,
                            NodeCallsConstants.NeoNotification);
                        break;
                    }
                }
            }

            if (newNode != null)
            {
                var newNodeAddress = new NodeAddress
                {
                    Ip = address.Address.ToMatchedIp(),
                    Node = newNode
                };

                var peer = db.Peers.FirstOrDefault(x => x.Ip == address.Address.ToMatchedIp());

                var result = LocationCaller.UpdateNode(newNode, newNodeAddress.Ip).GetAwaiter().GetResult();

                newNode.NodeAddresses.Add(newNodeAddress);

                db.NodeAddresses.Add(newNodeAddress);
                db.Nodes.Add(newNode);
                peer.Node = newNode;
                db.SaveChanges();
            }
        }

        private Node CreateNodeOfAddress(string ip, string protocol, string successUrl, NodeAddressType type, string service = null)
        {
            var result = new Node
            {
                Protocol = protocol,
                IsHttps = protocol.Equals("https"),
                SuccessUrl = successUrl,
                Type = type,
                Net = netSettings.Net,
                Url = ip.ToMatchedIp(),
                Service = service
            };

            return result;
        }
    }
}
