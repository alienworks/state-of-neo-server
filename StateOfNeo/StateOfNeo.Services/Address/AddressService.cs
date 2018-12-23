using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Address;
using StateOfNeo.ViewModels.Chart;
using X.PagedList;

namespace StateOfNeo.Services.Address
{
    public class AddressService : FilterService, IAddressService
    {
        public AddressService(StateOfNeoContext db)
            : base(db) { }

        public int CreatedAddressesCount() => this.db.Addresses.Count();

        public int CreatedAddressesCountForLast(UnitOfTime unit = UnitOfTime.Day)
        {
            // Skipping this date untill fully synced with blockchain
            //var date = DateTime.UtcNow;
            var date = this.db.Addresses
                .OrderByDescending(x => x.FirstTransactionOn)
                .Select(x => x.FirstTransactionOn)
                .First();

            var query = this.db.Addresses.AsQueryable();

            if (unit == UnitOfTime.Second)
            {
                query = query.Where(x => x.FirstTransactionOn > date.AddSeconds(-1));
            }
            if (unit == UnitOfTime.Hour)
            {
                query = query.Where(x => x.FirstTransactionOn > date.AddHours(-1));
            }
            if (unit == UnitOfTime.Day)
            {
                query = query.Where(x => x.FirstTransactionOn > date.AddDays(-1));
            }
            if (unit == UnitOfTime.Month)
            {
                query = query.Where(x => x.FirstTransactionOn > date.AddMonths(-1));
            }

            return query.Count();
        }

        public int ActiveAddressesInThePastThreeMonths()
        {
            var lastAddress = this.db.Addresses
                .OrderByDescending(x => x.LastTransactionOn)
                .First()
                .LastTransactionOn.AddMonths(-3);

            return this.db.Addresses
                 .Where(x => x.LastTransactionOn > lastAddress)
                 .Count();
        }

        public T Find<T>(string address) =>
            this.db.Addresses
                .Where(x => x.PublicAddress == address)
                .ProjectTo<T>()
                .FirstOrDefault();

        public IPagedList<AddressListViewModel> GetPage(int page = 1, int pageSize = 10)
        {
            var query = this.db.Addresses
                .Include(x => x.AddressesInAssetTransactions)
                .AsQueryable();

            return query
                .OrderByDescending(x => x.LastTransactionStamp)
                .ProjectTo<AddressListViewModel>()
                .ToPagedList(page, pageSize);
        }

        public IEnumerable<ChartStatsViewModel> GetAddressesForAssetChart(ChartFilterViewModel filter, string assetHash)
        {
            var query = this.db.Addresses.Where(x => x.Balances.Any(b => b.AssetHash == assetHash && b.Balance > 0));
            return this.GetChart(filter, query);
        }

        public IEnumerable<ChartStatsViewModel> GetCreatedAddressesChart(ChartFilterViewModel filter)
        {
            var query = this.db.Addresses.AsQueryable();
            return this.GetChart(filter, query);
        }

        public IEnumerable<AddressListViewModel> TopOneHundredNeo()
        {
            var result = this.db.AddressBalances
                .Include(x => x.Address)
                .Where(x => x.AssetHash == AssetConstants.NeoAssetId)
                .OrderByDescending(x => x.Balance)
                .Select(x => x.Address)
                .ProjectTo<AddressListViewModel>()
                .Take(100)
                .ToList();

            return result;
        }

        public IEnumerable<AddressListViewModel> TopOneHundredGas()
        {
            var result = this.db.AddressBalances
                .Where(x => x.AssetHash == AssetConstants.GasAssetId)
                .OrderByDescending(x => x.Balance)
                .Select(x => x.Address)
                .ProjectTo<AddressListViewModel>()
                .Take(100)
                .ToList();

            return result;
        }

        private IEnumerable<ChartStatsViewModel> GetChart(ChartFilterViewModel filter, IQueryable<Data.Models.Address> query)
        {
            if (filter.StartDate == null)
            {
                filter.StartDate = this.db.Addresses
                    .OrderByDescending(x => x.FirstTransactionOn)
                    .Select(x => x.FirstTransactionOn)
                    .FirstOrDefault();
            }

            var result = new List<ChartStatsViewModel>();
            query = query.Where(x => x.FirstTransactionOn >= filter.GetEndPeriod());

            if (filter.UnitOfTime == UnitOfTime.Hour)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.FirstTransactionOn.Year,
                    x.FirstTransactionOn.Month,
                    x.FirstTransactionOn.Day,
                    x.FirstTransactionOn.Hour
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, x.Key.Hour, 0, 0),
                    UnitOfTime = UnitOfTime.Hour,
                    Value = x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Day)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.FirstTransactionOn.Year,
                    x.FirstTransactionOn.Month,
                    x.FirstTransactionOn.Day
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day),
                    UnitOfTime = UnitOfTime.Day,
                    Value = x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Month)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.FirstTransactionOn.Year,
                    x.FirstTransactionOn.Month
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                    UnitOfTime = UnitOfTime.Month,
                    Value = x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }

            return result;
        }

        public IEnumerable<ChartStatsViewModel> GetTransactionTypesForAddress(ChartFilterViewModel filter, string address)
        {
            if (filter.StartDate == null)
            {
                filter.StartDate = this.db.Addresses
                    .OrderByDescending(x => x.FirstTransactionOn)
                    .Select(x => x.FirstTransactionOn)
                    .FirstOrDefault();
            }
            var query = this.db.Addresses.AsQueryable();
            var result = new List<ChartStatsViewModel>();
            query = query.Where(x => x.FirstTransactionOn >= filter.GetEndPeriod());

            if (filter.UnitOfTime == UnitOfTime.Hour)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.FirstTransactionOn.Year,
                    x.FirstTransactionOn.Month,
                    x.FirstTransactionOn.Day,
                    x.FirstTransactionOn.Hour
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, x.Key.Hour, 0, 0),
                    UnitOfTime = UnitOfTime.Hour,
                    Value = x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Day)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.FirstTransactionOn.Year,
                    x.FirstTransactionOn.Month,
                    x.FirstTransactionOn.Day
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day),
                    UnitOfTime = UnitOfTime.Day,
                    Value = x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Month)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.FirstTransactionOn.Year,
                    x.FirstTransactionOn.Month
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                    UnitOfTime = UnitOfTime.Month,
                    Value = x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }

            return result;
        }
    }
}
