using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using StateOfNeo.Data;
using StateOfNeo.Data.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using Neo.Network.P2P.Payloads;
using StateOfNeo.Data.Models.Enums;
using AutoMapper.QueryableExtensions;
using StateOfNeo.ViewModels.Chart;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;

namespace StateOfNeo.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly StateOfNeoContext db;

        public TransactionService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public T Find<T>(string hash) =>
            this.db.Transactions
                .Where(x => x.ScriptHash == hash)
                .ProjectTo<T>()
                .FirstOrDefault();

        public decimal TotalClaimed() =>
            this.db.Transactions
                .Any(x => x.Type == TransactionType.ClaimTransaction)
            ? this.db.Transactions
                .Include(x => x.Assets).ThenInclude(x => x.Asset)
                .Where(x => x.Type == TransactionType.ClaimTransaction)
                .SelectMany(x => x.Assets.Where(a => a.AssetType == Data.Models.Enums.AssetType.GAS))
                .Sum(x => x.Amount)
            : 0;

        public IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter)
        {
            if (filter.StartDate == null)
            {
                var dbTxLatestTime = this.db.Transactions
                    .OrderByDescending(x => x.Block.CreatedOn)
                    .Select(x => x.Block.CreatedOn)
                    .FirstOrDefault();

                filter.StartDate = dbTxLatestTime;
            }

            var query = this.db.Transactions.AsQueryable();
            var result = new List<ChartStatsViewModel>();

            query = query.Where(x => x.Block.Timestamp.ToUnixDate() >= filter.GetEndPeriod());

            if (filter.UnitOfTime == UnitOfTime.Hour)
            {
                result = query.GroupBy(x => new
                {
                    x.Block.Timestamp.ToUnixDate().Year,
                    x.Block.Timestamp.ToUnixDate().Month,
                    x.Block.Timestamp.ToUnixDate().Day,
                    x.Block.Timestamp.ToUnixDate().Hour
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
                result = query.GroupBy(x => new
                {
                    x.Block.Timestamp.ToUnixDate().Year,
                    x.Block.Timestamp.ToUnixDate().Month,
                    x.Block.Timestamp.ToUnixDate().Day
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
                result = query.GroupBy(x => new
                {
                    x.Block.Timestamp.ToUnixDate().Year,
                    x.Block.Timestamp.ToUnixDate().Month
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

        public IEnumerable<ChartStatsViewModel> GetPieStats()
        {
            return this.db.Transactions
                 .Select(x => x.Type)
                 .GroupBy(x => x)
                 .Select(x => new ChartStatsViewModel
                 {
                     Label = x.Key.ToString(),
                     Value = x.Count()
                 })
                 .ToList();
        }
    }
}
