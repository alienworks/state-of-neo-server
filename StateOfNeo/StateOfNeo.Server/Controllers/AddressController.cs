using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Server.Actors;
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

        public AddressController(IAddressService addresses, IPaginatingService paginating, IStateService state)
        {
            this.addresses = addresses;
            this.paginating = paginating;
            this.state = state;
        }

        [HttpGet("[action]/{address}")]
        public IActionResult Get(string address)
        {
            var result = this.addresses.Find<AddressDetailsViewModel>(address);
            if (result == null)
            {
                return this.BadRequest("Invalid address.");
            }

            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult List(int page = 1, int pageSize = 10)
        {
            var result = this.addresses.GetPage(page, pageSize);

            return this.Ok(result.ToListResult());
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
            return this.Ok(this.state.GetTotalAddressCount());
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
            var result = this.addresses.GetCreatedAddressesChart(filter);
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
