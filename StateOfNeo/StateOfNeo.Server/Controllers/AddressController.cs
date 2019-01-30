using Microsoft.AspNetCore.Mvc;
using Neo;
using StateOfNeo.Common;
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
using System.Collections.Generic;
using System.Linq;
using X.PagedList;

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
                var data = this.state.GetAddressesPage(page, pageSize);
                this.UpdateNeoAndGasBalances(data);

                var result = data.ToListResult();
                var extended = PagedListMetadataExtended.FromParent(result.MetaData);
                var pages = extended.TotalItemCount % pageSize == 0 
                    ? extended.TotalItemCount / pageSize 
                    : extended.TotalItemCount / pageSize + 1;

                extended.TotalItemCount = (int)this.state.MainStats.TotalStats.AddressCount;
                extended.PageCount = pages;
                result.MetaData = extended;

                return this.Ok(result);
            }
            else
            {
                var result = this.addresses.GetPage(page, pageSize);
                this.UpdateNeoAndGasBalances(result);

                return this.Ok(result.ToListResult());
            }
        }

        private void UpdateNeoAndGasBalances(IEnumerable<AddressListViewModel> result)
        {
            var resultList = result.ToList();
            for (int i = 0; i < resultList.Count; i++)
            {
                var balancesFromSnapshot = BlockchainBalances.GetGlobalAssets(resultList[i].Address);
                var balancesList = resultList[i].Balances.ToList();

                for (int j = 0; j < balancesList.Count; j++)
                {
                    if (balancesList[j].Name.ToLower() == "gas")
                    {
                        var gasKey = UInt256.Parse(AssetConstants.GasAssetId);
                        if (balancesFromSnapshot != null)
                        {
                            if (balancesFromSnapshot.ContainsKey(gasKey))
                            {
                                var balance = balancesFromSnapshot[gasKey];
                                balancesList[j].Balance = (decimal)balance;
                            }
                            else
                            {
                                balancesList[j].Balance = 0;
                            }
                        }
                    }
                    else if (balancesList[j].Name.ToLower() == "neo")
                    {
                        var neoKey = UInt256.Parse(AssetConstants.NeoAssetId);
                        if (balancesFromSnapshot != null)
                        {
                            if (balancesFromSnapshot.ContainsKey(neoKey))
                            {
                                var balance = balancesFromSnapshot[neoKey];
                                balancesList[j].Balance = (decimal)balance;
                            }
                            else
                            {
                                balancesList[j].Balance = 0;
                            }
                        }
                    }
                }
            }
        }

        [HttpGet("[action]")]
        public IActionResult TopNeo()
        {
            var result = this.addresses.TopOneHundredNeo();

            this.UpdateNeoAndGasBalances(result);

            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult TopGas()
        {
            var result = this.addresses.TopOneHundredGas();

            this.UpdateNeoAndGasBalances(result);

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
        public IActionResult ActiveChart([FromBody]ChartFilterViewModel filter)
        {
            var result = this.state.GetActiveAddressesChart(filter.UnitOfTime, filter.EndPeriod);
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
