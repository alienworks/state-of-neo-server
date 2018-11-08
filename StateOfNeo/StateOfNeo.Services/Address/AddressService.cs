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

namespace StateOfNeo.Services.Address
{
    public class AddressService : IAddressService
    {
        private readonly StateOfNeoContext db;

        public AddressService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public int CreatedAddressesCount() => this.db.Addresses.Count();

        public int ActiveAddressesInThePastThreeMonths() =>
            this.db.Addresses
                .Include(x => x.OutgoingTransactions).ThenInclude(tr => tr.Transaction).ThenInclude(x => x.Block)
                .Where(x => x.OutgoingTransactions.Any(tr => tr.Transaction.Block.Timestamp.ToUnixDate() > DateTime.UtcNow.AddMonths(-3)))
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

        public IEnumerable<ChartStatsViewModel> GetTransactionStats(string address)
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

        public IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter)
        {
            var query = this.db.Addresses.AsQueryable();
            var result = new List<ChartStatsViewModel>();

            if (filter.StartDate != null)
            {
                query = query.Where(x => x.FirstTransactionOn >= filter.StartDate);
            }

            if (filter.EndDate != null)
            {
                query = query.Where(x => x.FirstTransactionOn <= filter.EndDate);
            }

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
    }
}
