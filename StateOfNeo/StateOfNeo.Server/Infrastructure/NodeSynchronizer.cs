using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Cache;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class NodeSynchronizer
    {
        private NodeCache nodeCache;
        private StateOfNeoContext ctx;
        private RPCNodeCaller rPCNodeCaller;
        private LocationCaller locationCaller;
        private readonly IOptions<NetSettings> netsettings;
        public List<Node> CachedDbNodes;

        public NodeSynchronizer(NodeCache nodeCache,
            StateOfNeoContext ctx,
            RPCNodeCaller rPCNodeCaller,
            LocationCaller locationCaller,
            IOptions<NetSettings> netsettings)
        {
            this.nodeCache = nodeCache;
            this.ctx = ctx;
            this.rPCNodeCaller = rPCNodeCaller;
            this.locationCaller = locationCaller;
            this.netsettings = netsettings;
            this.UpdateDbCache();
        }

        public IEnumerable<T> GetCachedNodesAs<T>() =>
            this.CachedDbNodes.AsQueryable().ProjectTo<T>();

        private void UpdateDbCache() =>
            this.CachedDbNodes = this.ctx.Nodes
                .Include(n => n.NodeAddresses)
                .Where(n => n.Net.ToLower() == this.netsettings.Value.Net.ToLower())
                .Where(x => x.Type == NodeAddressType.RPC && string.IsNullOrEmpty(x.SuccessUrl) == false)
                .ToList();
        
        public async Task Init()
        {
            await this.UpdateNodesInformation();
            this.nodeCache.NodeList.Clear();
        }

        private void SyncCacheAndDb()
        {
            foreach (var cacheNode in nodeCache.NodeList)
            {
                var existingDbNode = this.CachedDbNodes
                    .FirstOrDefault(dbn => dbn.NodeAddresses.Any(ia => ia.Ip == cacheNode.Ip));

                if (existingDbNode == null)
                {
                    var newDbNode = Mapper.Map<Node>(cacheNode);
                    newDbNode.Type = NodeAddressType.P2P_TCP;
                    newDbNode.Net = netsettings.Value.Net;

                    this.ctx.Nodes.Add(newDbNode);
                    this.ctx.SaveChanges();

                    var nodeDbAddress = new NodeAddress
                    {
                        Ip = cacheNode.Ip,
                        Port = cacheNode.Port,
                        Type = NodeAddressType.P2P_TCP,

                        NodeId = newDbNode.Id
                    };

                    this.ctx.NodeAddresses.Add(nodeDbAddress);
                    this.ctx.SaveChanges();
                }
                else
                {
                    var portIsDifferent = existingDbNode.NodeAddresses.FirstOrDefault(na => na.Port == cacheNode.Port) == null;
                    if (portIsDifferent)
                    {
                        var nodeDbAddress = new NodeAddress
                        {
                            Ip = cacheNode.Ip,
                            Port = cacheNode.Port,
                            Type = NodeAddressType.P2P_TCP,

                            NodeId = existingDbNode.Id
                        };

                        this.ctx.NodeAddresses.Add(nodeDbAddress);
                        this.ctx.SaveChanges();
                    }
                }
            }
        }

        private async Task UpdateNodesInformation()
        {
            var dbNodes = this.ctx.Nodes
                    .Include(n => n.NodeAddresses)
                    .Where(n => n.Net.ToLower() == netsettings.Value.Net.ToLower())
                    .ToList();

            foreach (var dbNode in dbNodes)
            {
                if (dbNode.Type != NodeAddressType.REST)
                {
                    var oldSuccessUrl = dbNode.SuccessUrl;
                    var newHeight = await this.rPCNodeCaller.GetNodeHeight(dbNode);
                    if (newHeight != null)
                    {
                        dbNode.Type = NodeAddressType.RPC;
                        dbNode.Height = newHeight;

                        var newVersion = await this.rPCNodeCaller.GetNodeVersion(dbNode);
                        dbNode.Version = newVersion;

                        await this.locationCaller.UpdateNodeLocation(dbNode.Id);
                        //var peers = await _rPCNodeCaller.GetNodePeers(dbNode);
                        //if (peers != null)
                        //{
                        //    dbNode.Peers = peers.Connected.Count();
                        //}
                        
                        if (string.IsNullOrEmpty(dbNode.Net))
                        {
                            dbNode.Net = netsettings.Value.Net;
                        }

                        this.ctx.Nodes.Update(dbNode);
                        this.ctx.SaveChanges();
                    }
                }
            }

            this.UpdateDbCache();
        }
    }
}
