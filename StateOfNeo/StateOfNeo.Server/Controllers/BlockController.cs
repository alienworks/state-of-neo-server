using Microsoft.AspNetCore.Mvc;
using Neo.Ledger;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Services;
using StateOfNeo.Services.Block;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Block;
using StateOfNeo.ViewModels.Chart;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class BlockController : BaseApiController
    {
        private readonly IBlockService blocks;
        private readonly IPaginatingService paginating;
        private readonly IStateService state;

        public BlockController(IBlockService blocks, IPaginatingService paginating, IStateService state)
        {
            this.blocks = blocks;
            this.paginating = paginating;
            this.state = state;
        }

        [HttpGet("[action]/{hash}")]
        [ResponseCache(Duration = CachingConstants.TenYears)]
        public IActionResult HeightByHash(string hash)
        {
            var height = this.blocks.GetHeight(hash);
            return this.Ok(height);
        }

        [HttpGet("[action]/{hash}")]
        [ResponseCache(Duration = CachingConstants.TenYears)]
        public IActionResult ByHash(string hash)
        {
            var block = this.blocks.Find<BlockDetailsViewModel>(hash);
            if (block == null) return this.BadRequest("Invalid block hash");
            block.NextBlockHash = this.blocks.NextBlockHash(block.Height);
            return this.Ok(block);
        }

        [HttpGet("[action]/{height}")]
        public IActionResult ByHeight(int height)
        {
            var block = this.blocks.Find<BlockDetailsViewModel>(height);
            if (block == null) return this.BadRequest("Invalid block height");
            block.NextBlockHash = this.blocks.NextBlockHash(block.Height);
            return this.Ok(block);
        }

        [HttpGet("[action]/{hash}")]
        [ResponseCache(Duration = CachingConstants.TenYears)]
        public IActionResult StampByHash(string hash)
        {
            var block = this.blocks.Find<StampViewModel>(hash);
            if (block == null) return this.BadRequest("Invalid block hash");
            return this.Ok(block);
        }

        [HttpGet("[action]/{height}")]
        public IActionResult StampByHeight(int height)
        {
            var block = this.blocks.Find<StampViewModel>(height);
            if (block == null) return this.BadRequest("Invalid block height");
            return this.Ok(block);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List(int page = 1, int pageSize = 10)
        {
            var data = await this.paginating.GetPage<Data.Models.Block, BlockListViewModel>(page, pageSize, x => x.Height);
            var result = data.ToListResult();
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = CachingConstants.Day)]
        public IActionResult AverageTxCount()
        {
            return Ok(this.blocks.GetAvgTxCountPerBlock());
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = CachingConstants.Day)]
        public IActionResult AverageTime()
        {
            return Ok(this.blocks.GetAvgBlockTime());
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = CachingConstants.Day)]
        public IActionResult AverageSize()
        {
            return Ok(this.blocks.GetAvgBlockSize());
        }

        [HttpGet("[action]")]
        public IActionResult GetBCHeight()
        {
            return this.Ok(Blockchain.Singleton.Height.ToString());
        }

        [HttpPost("[action]")]
        [ResponseCache(Duration = CachingConstants.Hour)]
        public IActionResult BlockSizeChart([FromBody]ChartFilterViewModel filter)
        {
            //var result = this.blocks.GetBlockSizeStats(filter);
            var result = this.state.GetBlockSizesChart(filter.UnitOfTime, filter.Period);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        [ResponseCache(Duration = CachingConstants.Hour)]
        public IActionResult BlockTimeChart([FromBody]ChartFilterViewModel filter)
        {
            //var result = this.blocks.GetBlockTimeStats(filter);
            var result = this.state.GetBlockTimesChart(filter.UnitOfTime, filter.Period);
            return this.Ok(result);
        }
    }
}