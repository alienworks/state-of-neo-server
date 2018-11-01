using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common.Enums;
using StateOfNeo.Services;
using StateOfNeo.Services.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class AddressController : BaseApiController
    {
        private readonly IAddressService addresses;
        private readonly IPaginatingService paginating;

        public AddressController(IAddressService addresses, IPaginatingService paginating)
        {
            this.addresses = addresses;
            this.paginating = paginating;
        }

        [HttpGet("[action]/{period}")]
        public IActionResult Average(TimePeriod period)
        {
            var result = this.addresses.CreatedAddressesPer(period);
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult Top()
        {
            var result = this.addresses.TopOneHundred();
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
            var result = this.addresses.CreatedAddressesCount();
            return this.Ok(result);
        }
    }
}
