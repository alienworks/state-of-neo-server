using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neo.Ledger;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Server.Infrastructure;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class ValuesController : BaseApiController
    {
        private readonly StateOfNeoContext ctx;
        private readonly NodeSynchronizer nodeSynchronizer;
        private readonly LocationCaller locationCaller;
        private readonly NetSettings netSettings;

        public ValuesController(
            NodeSynchronizer nodeSynchronizer, 
            LocationCaller locationCaller, 
            StateOfNeoContext ctx, 
            IOptions<NetSettings> netSettings)
        {
            this.ctx = ctx;
            this.nodeSynchronizer = nodeSynchronizer;
            this.locationCaller = locationCaller;
            this.netSettings = netSettings.Value;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string ip)
        {
            var height = Blockchain.Singleton.Height;
            return Ok(height);
        }
    }
}
