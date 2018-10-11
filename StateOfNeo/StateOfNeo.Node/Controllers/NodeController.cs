using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Node.Cache;
using StateOfNeo.Node.Hubs;
using StateOfNeo.Node.Infrastructure;
using StateOfNeo.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Node.Controllers
{
    public class NodeController : BaseApiController
    {
        private readonly IHubContext<NodeHub> nodeHub;
        private readonly NodeCache nodeCache;
        private readonly NodeSynchronizer nodeSynchronizer;

        public NodeController(IHubContext<NodeHub> nodeHub, NodeCache nodeCache, NodeSynchronizer nodeSynchronizer)
        {
            this.nodeHub = nodeHub;
            this.nodeCache = nodeCache;
            this.nodeSynchronizer = nodeSynchronizer;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                //await this.nodeSynchronizer.Init();
                var nodes = this.nodeSynchronizer.GetCachedNodesAs<NodeViewModel>().ToList();
                return Ok(nodes);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
