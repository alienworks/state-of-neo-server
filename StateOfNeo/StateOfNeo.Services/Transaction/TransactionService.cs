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
using X.PagedList;
using StateOfNeo.ViewModels.Transaction;

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
                .Include(x => x.GlobalOutgoingAssets).ThenInclude(x => x.Asset)
                .Where(x => x.Type == TransactionType.ClaimTransaction)
                .SelectMany(x => x.GlobalOutgoingAssets.Where(a => a.AssetType == Data.Models.Enums.AssetType.GAS))
                .Sum(x => x.Amount)
            : 0;

        public IPagedList<TransactionListViewModel> TransactionsForAddress(string address, int page = 1, int pageSize = 10)
        {
            var incoming = this.db.Addresses
                .Include(x => x.IncomingTransactions).ThenInclude(x => x.OutGlobalTransaction).ThenInclude(x => x.Block)
                .Where(x => x.PublicAddress == address)
                .SelectMany(x => x.IncomingTransactions.Select(z => z.OutGlobalTransaction))
                .ProjectTo<TransactionListViewModel>();

            var outgoing = this.db.Addresses
                .Include(x => x.OutgoingTransactions).ThenInclude(x => x.InGlobalTransaction).ThenInclude(x => x.Block)
                .Where(x => x.PublicAddress == address)
                .SelectMany(x => x.OutgoingTransactions.Select(z => z.InGlobalTransaction))
                .ProjectTo<TransactionListViewModel>();

            var result = incoming
                .Union(outgoing)
                .Distinct()
                .OrderByDescending(x => x.Timestamp)
                .ToPagedList(page, pageSize);

            return result;
        }

        public IPagedList<T> GetPageTransactions<T>(int page = 1, int pageSize = 10, string blockHash = null)
        {
            var query = this.db.Transactions
                .OrderByDescending(x => x.Timestamp)
                .AsQueryable();

            if (!string.IsNullOrEmpty(blockHash))
            {
                query = query.Where(x => x.BlockId == blockHash);
            }

            return query
                .ProjectTo<T>()
                .ToPagedList(page, pageSize);
        }

        public IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter)
        {
            if (filter.StartDate == null)
            {
                var dbTxLatestTime = this.db.Transactions
                    .Include(x => x.Block)
                    .OrderByDescending(x => x.Timestamp)
                    .Select(x => x.Block.CreatedOn)
                    .FirstOrDefault();

                filter.StartDate = dbTxLatestTime;
            }

            var query = this.db.Transactions.Include(x => x.Block).AsQueryable();
            var result = new List<ChartStatsViewModel>();

            query = query.Where(x => x.Block.CreatedOn >= filter.GetEndPeriod());

            if (filter.UnitOfTime == UnitOfTime.Hour)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.Block.CreatedOn.Year,
                    x.Block.CreatedOn.Month,
                    x.Block.CreatedOn.Day,
                    x.Block.CreatedOn.Hour
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
                    x.Block.CreatedOn.Year,
                    x.Block.CreatedOn.Month,
                    x.Block.CreatedOn.Day
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
                    x.Block.CreatedOn.Year,
                    x.Block.CreatedOn.Month
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
