using StateOfNeo.Data;
using StateOfNeo.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Node.Cache
{
    public class NodeCache
    {
        private readonly StateOfNeoContext _ctx;

        public HashSet<NodeViewModel> NodeList { get; private set; }

        public IEnumerable<NodeViewModel> RpcEnabled => this.NodeList.Where(x => x.Type == "RPC").ToList();

        public NodeCache(StateOfNeoContext ctx)
        {
            _ctx = ctx;
            NodeList = new HashSet<NodeViewModel>();
        }

        public void Update(IEnumerable<NodeViewModel> nodeViewModels)
        {
            foreach (var node in nodeViewModels)
            {
                NodeList.Add(node);
            }
        }
    }
}
