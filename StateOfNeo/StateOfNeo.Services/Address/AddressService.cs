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

        public int ActiveAddressesInThePastThreeMonths() =>
            this.db.Addresses
                .Where(x => x.LastTransactionOn > DateTime.UtcNow.AddMonths(-3))
                .Count();

        public int CreatedAddressesPer(UnitOfTime timePeriod)
        {
            var result = 0;
            if (timePeriod == UnitOfTime.Hour)
            {
                result = this.db.Addresses.Count() / this.db.Addresses
                    .GroupBy(x => new { x.FirstTransactionOn.Year, x.FirstTransactionOn.Month, x.FirstTransactionOn.Day, x.FirstTransactionOn.Hour })
                    .Count();
            }
            else if (timePeriod == UnitOfTime.Day)
            {
                result = this.db.Addresses.Count() / this.db.Addresses
                    .GroupBy(x => new { x.FirstTransactionOn.Year, x.FirstTransactionOn.Month, x.FirstTransactionOn.Day })
                    .Count();
            }
            else if (timePeriod == UnitOfTime.Month)
            {
                result = this.db.Addresses.Count() / this.db.Addresses
                    .GroupBy(x => new { x.FirstTransactionOn.Year, x.FirstTransactionOn.Month })
                    .Count();
            }

            return result;
        }

        public T Find<T>(string address) =>
            this.db.Addresses
                .Where(x => x.PublicAddress == address)
                .ProjectTo<T>()
                .FirstOrDefault();

        public IPagedList<AddressListViewModel> GetPage(int page = 1, int pageSize = 10)
        {
            var query = this.db.Addresses
                .Include(x => x.OutgoingTransactions)
                .Include(x => x.IncomingTransactions)
                .AsQueryable();

            return query
                .OrderByDescending(x => x.LastTransactionOn)
                .ProjectTo<AddressListViewModel>()
                .ToPagedList(page, pageSize);
        }

        public IEnumerable<ChartStatsViewModel> GetTransactionsForAddressChart(string address)
        {
            return this.db.Transactions
                .Include(x => x.GlobalOutgoingAssets)
                .Include(x => x.GlobalIncomingAssets)
                .Include(x => x.Assets)
                .Where(x =>
                    x.GlobalIncomingAssets.Any(a => a.FromAddressPublicAddress == address || a.ToAddressPublicAddress == address)
                    || x.GlobalOutgoingAssets.Any(a => a.FromAddressPublicAddress == address || a.ToAddressPublicAddress == address)
                    || x.Assets.Any(a => a.FromAddressPublicAddress == address || a.ToAddressPublicAddress == address)
                )
                .Select(x => x.Type)
                .GroupBy(x => x)
                .Select(x => new ChartStatsViewModel
                {
                    Label = x.Key.ToString(),
                    Value = x.Count()
                })
                .ToList();
        }

        public IEnumerable<ChartStatsViewModel> GetAddressesForAssetChart(ChartFilterViewModel filter, string assetHash)
        {
            var query = this.db.Addresses.Where(x => x.Balances.Any(b => b.Asset.Hash == assetHash && b.Balance > 0));
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
                .Where(x => x.Asset.Hash == AssetConstants.NeoAssetId)
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
                .Include(x => x.Address)
                .Where(x => x.Asset.Hash == AssetConstants.GasAssetId)
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
    }
}
