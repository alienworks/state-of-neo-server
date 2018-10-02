using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Neo.Core;
using StateOfNeo.Server.Hubs;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class BlockController : BaseApiController
    {
        private readonly IHubContext<BlockHub> _blockHub;

        public BlockController(IHubContext<BlockHub> blockHub)
        {
            _blockHub = blockHub;
        }

        [HttpGet("[action]")]
        public IActionResult GetHeight()
        {
            return this.Ok(Blockchain.Default.Height.ToString());
        }

        [HttpPost]
        public async Task Post()
        {
            await _blockHub.Clients.All.SendAsync(Blockchain.Default.Height.ToString());
        }
    }
}
