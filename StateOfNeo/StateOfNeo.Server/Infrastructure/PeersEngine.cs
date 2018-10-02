using Neo.Network.P2P;
using StateOfNeo.Common;
using StateOfNeo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Akka.IO.Tcp;

namespace StateOfNeo.Server.Infrastructure
{
    public class PeersEngine
    {
        public ICollection<IPEndPoint> Peers { get; private set; }

        private readonly ICollection<TcpClient> PeerClients;

        public PeersEngine()
        {
            this.Peers = new HashSet<IPEndPoint>();
            this.PeerClients = new HashSet<TcpClient>();
        }

        public void AddNewPeers(ICollection<IPEndPoint> peers)
        {
            foreach (var peer in peers)
            {
                this.Peers.Add(peer);
            }
        }

        public async Task<ICollection<string>> CheckP2PStatus(ICollection<Node> nodes)
        {
            var failedAddresses = new HashSet<string>();
            foreach (var node in nodes)
            {
                try
                {
                    foreach (var address in node.NodeAddresses)
                    {
                        var peerIp = address.Ip.ToString().ToMatchedIp();
                        var weAreConnected = LocalNode.Singleton.GetRemoteNodes().Any(rn => rn.Remote.Address.ToString().ToMatchedIp() == peerIp);
                        if (weAreConnected)
                        {
                            continue;
                        }

                        var endPoint = new IPEndPoint(IPAddress.Parse(peerIp), 10333);
                        Program.NeoSystem.LocalNode.Tell(new Connect(endPoint), Program.NeoSystem.LocalNode);
                        var success = LocalNode.Singleton.GetRemoteNodes().Any(rn => rn.Remote.Address.ToString().ToMatchedIp() == peerIp);
                        if (success)
                        {
                            continue;
                        }

                        failedAddresses.Add(node.SuccessUrl);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return failedAddresses;
        }

        public void UpdateClients()
        {
            foreach (var peer in this.Peers)
            {
                try
                {
                    var peerIp = peer.Address.ToString().ToMatchedIp();
                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(new IPEndPoint(IPAddress.Parse(peerIp), peer.Port));

                    if (tcpClient.Connected)
                    {
                        var asd = "";
                    }
                }
                catch (Exception ex)
                {
                    var thereIsSomething = "";
                }
            }
        }
    }
}
