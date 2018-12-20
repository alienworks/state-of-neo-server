using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.ViewModels;
using StateOfNeo.Common.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using StateOfNeo.Services;
using StateOfNeo.ViewModels.Chart;
using Serilog;
using StateOfNeo.Common.Constants;

namespace StateOfNeo.Server.Controllers
{
    [ResponseCache(Duration = CachingConstants.Hour)]
    public class NodeController : BaseApiController
    {
        private readonly NodeCache nodeCache;
        private readonly NodeSynchronizer nodeSynchronizer;
        private readonly INodeService nodeService;

        public NodeController(
            NodeCache nodeCache,
            NodeSynchronizer nodeSynchronizer,
            INodeService nodeService)
        {
            this.nodeCache = nodeCache;
            this.nodeSynchronizer = nodeSynchronizer;
            this.nodeService = nodeService;
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

        [HttpGet("[action]")]
        public async Task<IActionResult> Update()
        {
            try
            {
                await this.nodeSynchronizer.Init();
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("[action]/{id}")]
        [ResponseCache(Duration = CachingConstants.Second)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var node = this.nodeSynchronizer.GetCachedNodesAs<NodeDetailsViewModel>().ToList().FirstOrDefault(x => x.Id == id);
                return Ok(node);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = CachingConstants.Second)]
        public async Task<IActionResult> Page(int page = 1, int pageSize = 10)
        {
            try
            {
                var nodes = await this.nodeService.GetPage<NodeViewModel>(page, pageSize);
                return Ok(nodes.ToListResult());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Consensus()
        {
            try
            {
                Log.Information("Getting Consensus data");
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
                    Log.Error("Couldn't load consensus list neo.org bad request");
                    return BadRequest();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Couldn't load consensus list {@ex}", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("[action]/{nodeId}")]
        public IActionResult LatencyChart([FromBody]ChartFilterViewModel filter, int nodeId)
        {
            var result = this.nodeService.LatencyChart(filter, nodeId);
            return Ok(result);
        }

        [HttpPost("[action]/{nodeId}")]
        public IActionResult PeersChart([FromBody]ChartFilterViewModel filter, int nodeId)
        {
            var result = this.nodeService.PeersChart(filter, nodeId);
            return Ok(result);
        }

        [HttpGet("[action]/{nodeId}")]
        public async Task<IActionResult> WsStatus(int nodeId)
        {
            var status = await this.nodeService.GetWsStatusAsync(nodeId);
            return this.Ok(status);
        }
    }
}
