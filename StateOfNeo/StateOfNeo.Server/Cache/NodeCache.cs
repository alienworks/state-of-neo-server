using AutoMapper.QueryableExtensions;
using StateOfNeo.Common.RPC;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Server.Cache
{
    public class NodeCache
    {
        public HashSet<NodeViewModel> NodeList { get; private set; }
        public HashSet<RPCPeer> PeersCollected { get; private set; }
        private List<Peer> PeersCached { get; set; }
        public IEnumerable<NodeViewModel> RpcEnabled => this.NodeList.Where(x => x.Type == "RPC").ToList();

        public NodeCache()
        {
            this.NodeList = new HashSet<NodeViewModel>();
            this.PeersCollected = new HashSet<RPCPeer>();
            this.PeersCached = new List<Peer>();
        }

        public void Update(IEnumerable<NodeViewModel> nodeViewModels)
        {
            foreach (var node in nodeViewModels)
            {
                this.NodeList.Add(node);
            }
        }

        public void AddPeer(RPCPeer peer)
        {
            this.PeersCollected.Add(peer);
        }

        public void AddPeerToCache(Peer peer)
        {
            this.PeersCached.Add(peer);
        }

        public void AddPeerToCache(ICollection<Peer> peers)
        {
            this.PeersCached.AddRange(peers);
        }

        public int GetCachedPeersCount => this.PeersCached.Count;

        public ICollection<T> GetCachedPeers<T>() => this.PeersCached.AsQueryable().ProjectTo<T>().ToList();
    }
}
