using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class NodeController : BaseApiController
    {
        private readonly NodeSynchronizer nodeSynchronizer;

        public NodeController(NodeSynchronizer nodeSynchronizer)
        {
            this.nodeSynchronizer = nodeSynchronizer;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
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
