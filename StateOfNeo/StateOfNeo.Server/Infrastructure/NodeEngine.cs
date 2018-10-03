using Neo.Network;
using StateOfNeo.ViewModels;
using StateOfNeo.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo.Network.P2P;

namespace StateOfNeo.Server.Infrastructure
{
    public static class NodeEngine
    {
        private static void BFSNodes(RemoteNode node, ref List<NodeViewModel> nodeViewModels)
        {
            var newNode = new NodeViewModel();
            var existingNode = nodeViewModels
                    .FirstOrDefault(
                        n => n.Ip == node.Remote.Address.ToString().ToMatchedIp() &&
                        n.Port == node.Remote.Port);

            if (existingNode != null)
            {
                existingNode.IsVisited = true;
            }

            var privateNode = ObjectExtensions.GetInstanceField<LocalNode>(typeof(RemoteNode), node, "localNode");
            
            if ((existingNode == null || !existingNode.IsVisited) && node.Version != null)
            {
                newNode = new NodeViewModel
                {
                    Ip = node.Remote.Address.ToString().ToMatchedIp(),
                    Port = node.Version?.Port != null ? node.Version.Port : (uint)node.Remote.Port,
                    Version = node.Version?.UserAgent,
                    Peers = privateNode.GetRemoteNodes().Count(),
                };

                nodeViewModels.Add(newNode);
                var nodes = privateNode.GetRemoteNodes();
                foreach (var remoteNode in nodes)
                {
                    BFSNodes(remoteNode, ref nodeViewModels);
                }
            }

            return;
        }

        public static List<NodeViewModel> GetNodesByBFSAlgo()
        {
            var result = new List<NodeViewModel>();
            var nodes = LocalNode.Singleton.GetRemoteNodes();
            foreach (var node in nodes)
            {
                BFSNodes(node, ref result);
            }

            return result;
        }
    }
}
