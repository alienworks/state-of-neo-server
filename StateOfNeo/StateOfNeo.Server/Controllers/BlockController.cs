using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Neo.Ledger;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Services;
using StateOfNeo.Services.Block;
using StateOfNeo.ViewModels.Block;
using StateOfNeo.ViewModels.Chart;
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
        public IActionResult ByHash(string hash)
        {
            var block = this.blocks.Find<BlockDetailsViewModel>(hash);
            if (block == null)
            {
                return this.BadRequest("Invalid block hash");
            }

            return this.Ok(block);
        }

        [HttpGet("[action]/{height}")]
        public IActionResult ByHeight(int height)
        {
            var block = this.blocks.Find<BlockDetailsViewModel>(height);
            if (block == null)
            {
                return this.BadRequest("Invalid block height");
            }

            return this.Ok(block);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List(int page = 1, int pageSize = 10)
        {
            var result = await this.paginating.GetPage<Data.Models.Block, BlockListViewModel>(page, pageSize, x => x.Height);

            return this.Ok(result.ToListResult());
        }

        [HttpPost("[action]")]
        public IActionResult BlockSizeChart([FromBody]ChartFilterViewModel filter)
        {
            var result = this.blocks.GetBlockSizeStats(filter);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult BlockTimeChart([FromBody]ChartFilterViewModel filter)
        {
            var result = this.blocks.GetBlockTimeStats(filter);
            return this.Ok(result);
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
