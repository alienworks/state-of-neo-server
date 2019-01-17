using Microsoft.AspNetCore.Mvc;
using Serilog;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.Services;
using StateOfNeo.ViewModels.Asset;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Server.Controllers
{
    public class AssetsController : BaseApiController
    {
        private readonly IPaginatingService paginating;
        private readonly IAssetService assets;

        public string GlobalConstants { get; private set; }

        public AssetsController(IPaginatingService paginating, IAssetService assets)
        {
            this.paginating = paginating;
            this.assets = assets;
        }

        [HttpGet("[action]/{hash}")]
        public IActionResult Get(string hash)
        {
            var asset = this.assets.Find<AssetDetailsViewModel>(hash);
            if (asset == null)
            {
                return this.BadRequest("Invalid asset hash");
            }

            return this.Ok(asset);
        }

        [HttpGet("[action]")]
        public IActionResult AddressesCount(string hash, UnitOfTime unitOfTime = UnitOfTime.None, bool active = false)
        {
            var result = this.assets.AddressCount(hash, unitOfTime, active);
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List(int page = 1, int pageSize = 10, bool global = true)
        {
            var result = await this.paginating.GetPage<Asset, AssetListViewModel>(
                page,
                pageSize,
                x => x.Hash == AssetConstants.NeoAssetId || x.Hash == AssetConstants.GasAssetId,
                x => global == true ? x.GlobalType != null : x.GlobalType == null);

            if (!global)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    var asset = result[i];
                    if (asset.Type == AssetType.NEP5)
                    {
                        try
                        {
                            var total = BlockchainBalances.GetTotalSupply(asset.Hash);
                            asset.TotalSupply = total;
                        }
                        catch (System.Exception e)
                        {
                            Log.Warning($"Total supply could not be called for {asset.Name} with hash {asset.Hash}");
                        }
                    }
                }
            }

            return this.Ok(result.ToListResult());
        }

        [HttpPost("[action]")]
        public IActionResult Count(AssetType[] types)
        {
            int result = this.assets.Count(types);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult TxCount(AssetType[] types)
        {
            int result = this.assets.TxCount(types);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult TokenChart()
        {
            var result = this.assets.TokenChart();
            return this.Ok(result);
        }
    }
}
