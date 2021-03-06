﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateOfNeo.Common;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Data.Seed
{
    public class StateOfNeoSeedData
    {
        private readonly StateOfNeoContext db;
        private readonly IOptions<NetSettings> netSettings;

        public StateOfNeoSeedData(StateOfNeoContext ctx, IOptions<NetSettings> netSettings)
        {
            db = ctx;
            this.netSettings = netSettings;
        }

        public void Init()
        {
            this.SeedPeers();
            this.SeedNodesInPeers();
            this.SeedNodes();
            this.SeedAddresses();
        }

        private void SeedNodesInPeers()
        {
            if (!this.db.Peers.Any(x => x.NodeId.HasValue))
            {
                var addresses = this.db.NodeAddresses
                    .Include(x => x.Node)
                    .Where(x => x.Node.Net == this.netSettings.Value.Net)
                    .ToList();
                var peers = this.db.Peers.ToList();

                foreach (var peer in peers)
                {
                    var address = addresses.FirstOrDefault(x => x.Ip.ToMatchedIp() == peer.Ip.ToMatchedIp());
                    if (address != null)
                    {
                        peer.NodeId = address.NodeId;
                    }
                }

                this.db.SaveChanges();
            }
        }

        private void SeedPeers()
        {
            if (!this.db.Peers.Any())
            {
                var nodes = this.db.Nodes.Include(x => x.NodeAddresses).ToList();

                foreach (var node in nodes)
                {
                    foreach (var address in node.NodeAddresses)
                    {
                        if (!this.db.Peers.Any(x => x.Ip == address.Ip))
                        {
                            var newPeer = new Peer
                            {
                                Ip = address.Ip,
                                FlagUrl = node.FlagUrl,
                                Latitude = node.Latitude,
                                Longitude = node.Longitude,
                                Locale = node.Locale,
                                Location = node.Location
                            };

                            this.db.Peers.Add(newPeer);
                            this.db.SaveChanges();
                        }
                    }
                }
            }
        }

        private void SeedNodes()
        {
            if (!this.db.Nodes.Any())
            {
                SeedNodesByNetType(NetConstants.MAIN_NET);
                SeedNodesByNetType(NetConstants.TEST_NET);
            }
        }

        private void SeedNodesByNetType(string net)
        {
            var seedNodesFileName = $@"seed-{net.ToLower()}.json";
            var seedNodesSerialized = File.ReadAllText(seedNodesFileName);
            var seedNodes = JsonConvert.DeserializeObject<List<NodeViewModel>>(seedNodesSerialized);
            foreach (var node in seedNodes)
            {
                var newNode = new Node
                {
                    Id = 0,
                    Net = net,
                    Locale = node.Locale,
                    Location = node.Location,
                    Protocol = node.Protocol,
                    Url = node.Url,
                    Type = Enum.Parse<NodeAddressType>(node.Type),
                    Version = node.Version,
                    Service = node.Service
                };

                this.db.Nodes.Add(newNode);
                this.db.SaveChanges();

                RegisterIpAddresses(newNode.Id, node);
            }
        }

        private void RegisterIpAddresses(int nodeId, NodeViewModel node)
        {
            if (node.Ips != null)
            {
                foreach (var ip in node.Ips)
                {
                    var newAddress = new NodeAddress
                    {
                        Ip = ip,
                        Port = node.Port,
                        NodeId = nodeId,
                        Type = Enum.Parse<NodeAddressType>(node.Type)
                    };

                    this.db.NodeAddresses.Add(newAddress);
                    this.db.SaveChanges();
                }
            }
            else if (node.Ip != null)
            {
                var newAddress = new NodeAddress
                {
                    Ip = node.Ip,
                    Port = node.Port,
                    NodeId = nodeId,
                    Type = Enum.Parse<NodeAddressType>(node.Type)
                };

                this.db.NodeAddresses.Add(newAddress);
                this.db.SaveChanges();
            }
        }

        private void SeedAddresses()
        {
            if (!this.db.NodeAddresses.Any() && this.db.Nodes.Any())
            {
                var mainNodes = ((JArray)JsonConvert.DeserializeObject(File.ReadAllText($@"seed-{NetConstants.MAIN_NET.ToLower()}.json"))).ToObject<List<NodeViewModel>>();
                var dbNodes = this.db.Nodes.Where(x => x.Net == NetConstants.MAIN_NET).ToList();

                for (int i = 0; i < dbNodes.Count; i++)
                {
                    var mainNode = mainNodes.FirstOrDefault(x => x.Url.Equals(dbNodes[i].Url));
                    if (mainNode != null)
                    {
                        this.RegisterIpAddresses(dbNodes[i].Id, mainNode);
                    }
                }
            }
        }
    }
}
