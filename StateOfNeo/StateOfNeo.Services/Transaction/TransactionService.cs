using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Neo.Network.P2P.Payloads;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Chart;
using StateOfNeo.ViewModels.Transaction;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IPagedList<TransactionListViewModel> TransactionsForAsset(string asset, int page = 1, int pageSize = 10) =>
            this.db.Transactions
                .Where(x => 
                    x.Assets.Any(a => a.Asset.Hash == asset)
                    || x.GlobalIncomingAssets.Any(a => a.Asset.Hash == asset)
                    || x.GlobalIncomingAssets.Any(a => a.Asset.Hash == asset))
                .OrderByDescending(x => x.Timestamp)
                .ProjectTo<TransactionListViewModel>()
                .ToPagedList(page, pageSize);

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

        public IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter) => 
            this.Filter<Data.Models.Transactions.Transaction>(filter);
        
        public IEnumerable<ChartStatsViewModel> GetTransactionsForAssetChart(ChartFilterViewModel filter, string assetHash) =>
            this.Filter<Data.Models.Transactions.Transaction>(
                filter,
                null,
                x =>
                    x.Assets.Any(a => a.Asset.Hash == assetHash)
                    || x.GlobalIncomingAssets.Any(a => a.Asset.Hash == assetHash)
                    || x.GlobalOutgoingAssets.Any(a => a.Asset.Hash == assetHash));

        public IEnumerable<ChartStatsViewModel> GetTransactionsForAddressChart(ChartFilterViewModel filter, string address) =>
            this.Filter<Data.Models.Transactions.Transaction>(
                filter,
                null,
                x =>
                    x.Assets.Any(a => a.FromAddress.PublicAddress == address || a.ToAddress.PublicAddress == address)
                    || x.GlobalIncomingAssets.Any(a => a.FromAddress.PublicAddress == address || a.ToAddress.PublicAddress == address)
                    || x.GlobalOutgoingAssets.Any(a => a.FromAddress.PublicAddress == address || a.ToAddress.PublicAddress == address));

        public IEnumerable<ChartStatsViewModel> GetPieStats() =>
            this.db.Transactions
                 .Select(x => x.Type)
                 .GroupBy(x => x)
                 .Select(x => new ChartStatsViewModel
                 {
                     Label = x.Key.ToString(),
                     Value = x.Count()
                 })
                 .ToList();

        public IEnumerable<ChartStatsViewModel> GetTransactionTypesForAddress(string address) =>
            this.db.Transactions
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
        }
        public long Total() => this.db.Transactions.Count();
            
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
