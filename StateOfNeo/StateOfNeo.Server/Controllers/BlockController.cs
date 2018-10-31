using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Neo.Ledger;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Services;
using StateOfNeo.Services.Block;
using StateOfNeo.ViewModels.Block;
using System;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class BlockController : BaseApiController
    {
        private readonly IHubContext<BlockHub> blockHub;
        private readonly IBlockService blocks;
        private readonly IPaginatingService paginating;

        public BlockController(IHubContext<BlockHub> blockHub, IBlockService blocks, IPaginatingService paginating)
        {
            this.blockHub = blockHub;
            this.blocks = blocks;
            this.paginating = paginating;
        }

        [HttpGet("[action]/{hash}")]
        public IActionResult Get(string hash)
        {
            var block = this.blocks.Find(hash);
            if (block == null)
            {
                return this.BadRequest("Invalid block hash");
            }

            var result = Mapper.Map<BlockDetailsViewModel>(block);

            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List(int page = 1, int pageSize = 10)
        {
            var result = await this.paginating.GetPage<Data.Models.Block, BlockListViewModel>(page, pageSize, x => x.Height);         

            return this.Ok(result.ToListResult());
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
