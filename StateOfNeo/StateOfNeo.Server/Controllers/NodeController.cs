using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.ViewModels;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
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

        [HttpGet("[action]")]
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

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var node = this.nodeSynchronizer.GetCachedNodesAs<NodeViewModel>().ToList().FirstOrDefault(x => x.Id == id);
                return Ok(node);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = 60 * 60)]
        public async Task<IActionResult> Consensus()
        {
            try
            {
                HttpResponseMessage response;

                using (var http = new HttpClient())
                {
                    var method = HttpMethod.Get;
                    var req = new HttpRequestMessage(method, $"https://neo.org/consensus/getvalidators");
                    
                    response = await http.SendAsync(req);
                }

                if (response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<object[]>(text);
                    return Ok(list);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
