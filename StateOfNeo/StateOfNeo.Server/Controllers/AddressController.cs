using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Server.Actors;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.Services;
using StateOfNeo.Services.Address;
using StateOfNeo.ViewModels.Address;
using StateOfNeo.ViewModels.Chart;

namespace StateOfNeo.Server.Controllers
{
    public class AddressController : BaseApiController
    {
        private readonly IAddressService addresses;
        private readonly IPaginatingService paginating;
        private readonly IStateService state;
        private readonly StateOfNeoContext db;

        public AddressController(IAddressService addresses,
            IPaginatingService paginating,
            IStateService state,
            StateOfNeoContext db)
        {
            this.addresses = addresses;
            this.paginating = paginating;
            this.state = state;
            this.db = db;
        }

        [HttpGet("[action]/{address}")]
        public IActionResult Get(string address)
        {
            //var result = this.addresses.Find<AddressDetailsViewModel>(address);
            var result = AutoMapper.Mapper.Map<AddressDetailsViewModel>(BlockchainBalances.GetAddressAssets(address, this.db));
            if (result == null)
            {
                return this.BadRequest("Invalid address.");
            }

            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult List(int page = 1, int pageSize = 10)
        {
            if (page * pageSize <= StateService.CachedAddressesCount)
            {
                var result = this.state.GetAddressesPage(page, pageSize);
                return this.Ok(result.ToListResult());
            }
            else
            {
                var result = this.addresses.GetPage(page, pageSize);
                return this.Ok(result.ToListResult());
            }
        }

        [HttpGet("[action]")]
        public IActionResult TopNeo()
        {
            var result = this.addresses.TopOneHundredNeo();
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult TopGas()
        {
            var result = this.addresses.TopOneHundredGas();
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult Active()
        {
            var result = this.addresses.ActiveAddressesInThePastThreeMonths();
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult Created()
        {
            return this.Ok(this.state.MainStats.GetTotalAddressCount());
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = CachingConstants.Hour)]
        public IActionResult CreatedLast([FromQuery]UnitOfTime unit)
        {
            int result = this.addresses.CreatedAddressesCountForLast(unit);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        [ResponseCache(Duration = CachingConstants.Hour)]
        public IActionResult Chart([FromBody]ChartFilterViewModel filter)
        {
            //var result = this.addresses.GetCreatedAddressesChart(filter);
            var result = this.state.GetAddressesChart(filter.UnitOfTime, filter.EndPeriod);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        [ResponseCache(Duration = CachingConstants.Hour)]
        public IActionResult AssetChart([FromBody]ChartFilterViewModel filter, string assetHash)
        {
            var result = this.addresses.GetAddressesForAssetChart(filter, assetHash);
            return this.Ok(result);
        }
    }
}
