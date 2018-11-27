using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Neo.Network.P2P.Payloads;
using Serilog;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Helpers.Filters;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Chart;
using StateOfNeo.ViewModels.Transaction;
using System;
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
                .Where(x => x.Hash == hash)
                .ProjectTo<T>()
                .FirstOrDefault();

        public IPagedList<TransactionListViewModel> TransactionsForAddress(string address, int page = 1, int pageSize = 10) =>
            this.db.AddressesInTransactions
                .Where(x => x.AddressPublicAddress == address)
                .Select(x => x.Transaction)
                .ProjectTo<TransactionListViewModel>()
                .ToPagedList(page, pageSize);


        public IPagedList<TransactionListViewModel> TransactionsForAsset(string asset, int page = 1, int pageSize = 10) =>
            this.db.AssetsInTransactions
                .Where(x => x.AssetHash == asset)
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Transaction)
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

        public IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var latestDate = this.db.Transactions
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Timestamp)
                .First();

            filter.StartDate = latestDate.ToUnixDate();
            filter.StartStamp = latestDate;

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            var periods = filter.GetPeriodStamps();
            foreach (var endStamp in periods)
            {
                var count = this.db.Transactions
                    .Where(x => x.Timestamp <= latestDate && x.Timestamp >= endStamp)
                    .Count();

                result.Add(new ChartStatsViewModel
                {
                    Value = (decimal)count,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                latestDate = endStamp;
            }

            stopwatch.Stop();
            Log.Information($"{this.GetType().FullName} - GetStats time - " + stopwatch.ElapsedMilliseconds);
            return result;

            //this.ConfirmStartDateValue<Data.Models.Transactions.Transaction>(filter);
            //var query = this.db.Set<Data.Models.Transactions.Transaction>().AsQueryable();
            //IQueryable<IGrouping<long, Data.Models.Transactions.Transaction>> grouping = null;
            //if (filter.UnitOfTime == UnitOfTime.Hour)
            //{
            //    grouping = query.GroupBy(x => x.HourlyStamp);
            //}
            //else if (filter.UnitOfTime == UnitOfTime.Day)
            //{
            //    grouping = query.GroupBy(x => x.DailyStamp);
            //}
            //else if (filter.UnitOfTime == UnitOfTime.Month)
            //{
            //    grouping = query.GroupBy(x => x.MonthlyStamp);
            //}

            //var result = grouping
            //    .OrderByDescending(x => x.Key)
            //    .Take(filter.EndPeriod)
            //    .Select(x => new ChartStatsViewModel
            //    {
            //        StartDate = x.Key.ToUnixDate(),
            //        UnitOfTime = filter.UnitOfTime,
            //        Value = x.Count()
            //    })
            //    .ToList();

            //result.Reverse();

            //return result;
        }

        public IEnumerable<ChartStatsViewModel> GetTransactionsForAssetChart(ChartFilterViewModel filter, string assetHash)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var latestDate = this.db.Transactions
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Timestamp)
                .First();

            filter.StartDate = latestDate.ToUnixDate();
            filter.StartStamp = latestDate;

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            var periods = filter.GetPeriodStamps();
            foreach (var endStamp in periods)
            {
                var count = this.db.AssetsInTransactions
                    .Where(x => x.AssetHash == assetHash)
                    .Select(x => x.Timestamp)
                    .Count(x => x <= latestDate && x >= endStamp);

                result.Add(new ChartStatsViewModel
                {
                    Value = (decimal)count,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                latestDate = endStamp;
            }

            stopwatch.Stop();
            Log.Information($"{this.GetType().FullName} - GetStats time - " + stopwatch.ElapsedMilliseconds);
            return result;

            //return this.Filter<Data.Models.Transactions.Transaction>(
            //      filter,
            //      null,
            //      x =>
            //          );
        }

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

        public long Total() => this.db.Transactions.Count();

        public decimal TotalClaimed() =>
            this.db.Transactions
                .Include(x => x.GlobalOutgoingAssets)
                .Where(x => x.Type == TransactionType.ClaimTransaction)
                .SelectMany(x => x.GlobalOutgoingAssets)
                .Sum(x => x.Amount);
    }
}
