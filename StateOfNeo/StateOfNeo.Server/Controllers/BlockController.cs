using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Neo.Ledger;
using StateOfNeo.Server.Hubs;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class BlockController : BaseApiController
    {
        private readonly IHubContext<BlockHub> blockHub;

        public BlockController(IHubContext<BlockHub> blockHub)
        {
            this.blockHub = blockHub;
        }

        [HttpGet("[action]")]
        public IActionResult GetHeight()
        {
            return this.Ok(Blockchain.Singleton.Height.ToString());
        }

        [HttpPost]
        public async Task Post()
        {
            await blockHub.Clients.All.SendAsync(Blockchain.Singleton.Height.ToString());
        }
    }
}
