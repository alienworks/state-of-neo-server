using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Neo.Ledger;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class ValuesController : BaseApiController
    {
        private readonly IHubContext<StatsHub> statsHub;

        public ValuesController(IHubContext<StatsHub> statsHub)
        {
            this.statsHub = statsHub;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string ip)
        {
            var height = Blockchain.Singleton.Height;
            return Ok(height);
        }
    }
}
