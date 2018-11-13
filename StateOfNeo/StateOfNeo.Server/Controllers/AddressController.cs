﻿using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Services;
using StateOfNeo.Services.Address;
using StateOfNeo.ViewModels.Address;
using StateOfNeo.ViewModels.Chart;
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

        [HttpGet("[action]/{period}")]
        public IActionResult Average(UnitOfTime period)
        {
            var result = this.addresses.CreatedAddressesPer(period);
            return this.Ok(result);
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
            var result = this.addresses.CreatedAddressesCount();
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult CreatedLast([FromQuery]UnitOfTime unit)
        {
            int result = this.addresses.CreatedAddressesCountForLast(unit);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult Chart([FromBody]ChartFilterViewModel filter)
        {
            var result = this.addresses.GetCreatedAddressesChart(filter);
            return this.Ok(result);
        }
        
        [HttpPost("[action]")]
        public IActionResult AssetChart([FromBody]ChartFilterViewModel filter, string assetHash)
        {
            var result = this.addresses.GetAddressesForAssetChart(filter, assetHash);
            return this.Ok(result);
        }
    }
}
