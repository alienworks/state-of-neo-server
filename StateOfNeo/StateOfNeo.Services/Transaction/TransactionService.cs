using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Neo.Network.P2P.Payloads;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Chart;
using StateOfNeo.ViewModels.Transaction;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;

namespace StateOfNeo.Services.Transaction
{
    public class TransactionService : FilterService, ITransactionService
    {
        public TransactionService(StateOfNeoContext db) : base(db) { }

        public T Find<T>(string hash) =>
            this.db.Transactions
                .Where(x => x.ScriptHash == hash)
                .ProjectTo<T>()
                .FirstOrDefault();
        
        public IPagedList<TransactionListViewModel> TransactionsForAddress(string address, int page = 1, int pageSize = 10)
        {
            var globalIncoming = this.db.TransactedAssets
                .Include(x => x.InGlobalTransaction)
                .Where(x => (x.ToAddressPublicAddress == address || x.FromAddressPublicAddress == address) && x.InGlobalTransaction != null)
                .Select(x => x.InGlobalTransaction);

            var globalOutgoing = this.db.TransactedAssets
                .Include(x => x.OutGlobalTransaction)
                .Where(x => (x.ToAddressPublicAddress == address || x.FromAddressPublicAddress == address) && x.OutGlobalTransaction != null)
                .Select(x => x.OutGlobalTransaction);

            var nepTransactions = this.db.TransactedAssets
                .Include(x => x.Transaction)
                .Where(x => (x.ToAddressPublicAddress == address || x.FromAddressPublicAddress == address) && x.Transaction != null)
                .Select(x => x.Transaction);

            var result = globalIncoming
                .Union(globalOutgoing)
                .Union(nepTransactions)
                .ProjectTo<TransactionListViewModel>()
                .OrderByDescending(x => x.Timestamp)
                .ToPagedList(page, pageSize);

            return result;
        }

        public IPagedList<TransactionListViewModel> TransactionsForAsset(string asset, int page = 1, int pageSize = 10)
        {
            var globalIncoming = this.db.TransactedAssets
                .Include(x => x.InGlobalTransaction)
                .Include(x => x.Asset)
                .Where(x => x.Asset.Hash == asset && x.InGlobalTransaction != null)
                .Select(x => x.InGlobalTransaction);

            var globalOutgoing = this.db.TransactedAssets
                .Include(x => x.OutGlobalTransaction)
                .Include(x => x.Asset)
                .Where(x => x.Asset.Hash == asset && x.OutGlobalTransaction != null)
                .Select(x => x.OutGlobalTransaction);

            var nepTransactions = this.db.TransactedAssets
                .Include(x => x.Transaction)
                .Include(x => x.Asset)
                .Where(x => x.Asset.Hash == asset && x.Transaction != null)
                .Select(x => x.Transaction);

            var result = globalIncoming
                .Union(globalOutgoing)
                .Union(nepTransactions)
                .ProjectTo<TransactionListViewModel>()
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
            return this.Filter<Data.Models.Transactions.Transaction>(filter);
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

        public double AveragePer(UnitOfTime unitOfTime)
        {
            var total = this.Total();
            if (total > 0)
            {
                var since = this.db.Transactions
                    .Where(x => x.Timestamp != 0)
                    .OrderBy(x => x.Timestamp)
                    .Select(x => x.Timestamp)
                    .First().ToUnixDate();
                var end = this.db.Transactions
                    .Where(x => x.Timestamp != 0)
                    .OrderByDescending(x => x.Timestamp)
                    .Select(x => x.Timestamp)
                    .First().ToUnixDate();
                double timeFrames = 1;

                if (unitOfTime == UnitOfTime.Second)
                {
                    timeFrames = (end - since).TotalSeconds;
                }
                if (unitOfTime == UnitOfTime.Hour)
                {
                    timeFrames = (end - since).TotalHours;
                }
                if (unitOfTime == UnitOfTime.Day)
                {
                    timeFrames = (end - since).TotalDays;
                }

                var result = total / timeFrames;
                return result;
            }

            return 0;
        }

        public long Total()
        {
            var total = this.db.Transactions.Count();
            return total;
        }

        public decimal TotalClaimed() =>
            this.db.Transactions
                .Any(x => x.Type == TransactionType.ClaimTransaction)
            ? this.db.Transactions
                .Include(x => x.GlobalOutgoingAssets).ThenInclude(x => x.Asset)
                .Where(x => x.Type == TransactionType.ClaimTransaction)
                .SelectMany(x => x.GlobalOutgoingAssets.Where(a => a.AssetType == Common.Enums.AssetType.GAS))
                .Sum(x => x.Amount)
            : 0;
    }
}
