using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StateOfNeo.Common;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Helpers.Filters;
using StateOfNeo.Data;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Chart;
using System;
using StateOfNeo.ViewModels.Hub;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Serilog;
using Neo.Ledger;
using Neo;
using StateOfNeo.ViewModels.Address;
using X.PagedList;
using StateOfNeo.ViewModels.Transaction;
using Neo.Network.P2P.Payloads;

namespace StateOfNeo.Services
{
    public class StateService : IStateService
    {
        public const int CachedAddressesCount = 500;
        public const int CachedTransactionsCount = 500;

        private Dictionary<string, Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>>> charts = new Dictionary<string, Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>>>();
        private ICollection<ChartStatsViewModel> transactionTypes = new List<ChartStatsViewModel>();
        private DateTime? transactionTypesLastUpdate;
        private List<AddressListViewModel> addresses;
        private List<TransactionListViewModel> transactions;

        private long? lastBlockTime;
        private readonly StateOfNeoContext db;
        private readonly List<ChartStatsViewModel> dbCharts;

        public IMainStatsState MainStats { get; }
        public IContractsState Contracts { get; }

        public StateService(IOptions<DbSettings> dbOptions, IMainStatsState mainStats)
        {
            this.MainStats = mainStats;

            Stopwatch stopwatch = Stopwatch.StartNew();
            this.db = StateOfNeoContext.Create(dbOptions.Value.DefaultConnection);

            this.Contracts = new ContractsState();

            this.GetLatestTimestamp();
            this.LoadTransactionTypes();
            this.LoadLastActiveAddresses();
            this.LoadLastTransactions();

            this.LoadTransactionsMainChart();
            this.LoadCreatedAddressesMainChart();
            this.LoadBlockTimesMainChart();

            this.LoadBlockSizesMainChart();

            stopwatch.Stop();
            Log.Information($"{nameof(StateService)} initialization {stopwatch.ElapsedMilliseconds} ms");
        }

        private List<ChartStatsViewModel> GetChartEntries(UnitOfTime unitOfTime, ChartEntryType type) =>
            this.db.ChartEntries
                .Where(x => x.UnitOfTime == unitOfTime && x.Type == type)
                .OrderByDescending(x => x.Timestamp)
                .Take(36)
                .ProjectTo<ChartStatsViewModel>()
                .ToList();

        public ICollection<ChartStatsViewModel> GetAssetsChart(UnitOfTime unitOfTime, int count) =>
            this.GetChart("assets")[unitOfTime].OrderByDescending(x => x.StartDate).Take(count).ToList();

        public ICollection<ChartStatsViewModel> GetAddressesChart(UnitOfTime unitOfTime, int count) =>
            this.GetChart("createdAddresses")[unitOfTime].OrderByDescending(x => x.StartDate).Take(count).ToList();

        public ICollection<ChartStatsViewModel> GetTransactionsChart(UnitOfTime unitOfTime, int count) =>
            this.GetChart("transactions")[unitOfTime].OrderByDescending(x => x.StartDate).Take(count).ToList();

        public ICollection<ChartStatsViewModel> GetBlockSizesChart(UnitOfTime unitOfTime, int count) =>
            this.GetChart("blockSizes")[unitOfTime]
                .OrderByDescending(x => x.StartDate)
                .Take(count)
                .Where(x => x.Value > 0)
                .Select(x => new ChartStatsViewModel { Label = x.Label, StartDate = x.StartDate, Value = x.AccumulatedValue / x.Value })
                .ToList();

        public ICollection<ChartStatsViewModel> GetBlockTimesChart(UnitOfTime unitOfTime, int count) =>
            this.GetChart("blockTimes")[unitOfTime]
                .OrderByDescending(x => x.StartDate)
                .Take(count)
                .Where(x => x.Value > 0)
                .Select(x => new ChartStatsViewModel { Label = x.Label, StartDate = x.StartDate, Value = x.AccumulatedValue / x.Value })
                .ToList();

        public void AddBlockTime(double blockSeconds, DateTime time)
        {
            this.AddChartValues("blockTimes", 1, time, UnitOfTime.Hour, blockSeconds);
            this.AddChartValues("blockTimes", 1, time, UnitOfTime.Day, blockSeconds);
            this.AddChartValues("blockTimes", 1, time, UnitOfTime.Month, blockSeconds);
        }

        public void AddBlockSize(int size, DateTime time)
        {
            this.AddChartValues("blockSizes", 1, time, UnitOfTime.Hour, size);
            this.AddChartValues("blockSizes", 1, time, UnitOfTime.Day, size);
            this.AddChartValues("blockSizes", 1, time, UnitOfTime.Month, size);
        }

        public ICollection<ChartStatsViewModel> GetBlockTransactionsChart(UnitOfTime unitOfTime, int count) =>
            this.GetChart("blockTransactions")[unitOfTime].OrderByDescending(x => x.StartDate).Take(count).ToList();

        public void AddBlockTransactions(int transactions, DateTime time)
        {
            this.AddChartValues("blockTransactions", 1, time, UnitOfTime.Hour, transactions);
            this.AddChartValues("blockTransactions", 1, time, UnitOfTime.Day, transactions);
            this.AddChartValues("blockTransactions", 1, time, UnitOfTime.Month, transactions);
        }


        public void AddAddresses(int count, DateTime time)
        {
            this.AddChartValues("createdAddresses", count, time, UnitOfTime.Hour);
            this.AddChartValues("createdAddresses", count, time, UnitOfTime.Day);
            this.AddChartValues("createdAddresses", count, time, UnitOfTime.Month);
        }

        public void AddTransactions(int count, DateTime time)
        {
            this.AddChartValues("transactions", count, time, UnitOfTime.Hour);
            this.AddChartValues("transactions", count, time, UnitOfTime.Day);
            this.AddChartValues("transactions", count, time, UnitOfTime.Month);
        }

        public IEnumerable<ChartStatsViewModel> GetTransactionsPer(UnitOfTime unitOfTime, int count) =>
            this.charts["transactions"][unitOfTime].OrderByDescending(x => x.StartDate).Take(count);

        public IEnumerable<ChartStatsViewModel> GetTransactionTypes()
        {
            if (this.transactionTypesLastUpdate == null
                || this.transactionTypesLastUpdate.Value.AddHours(6) <= DateTime.UtcNow)
            {
                this.LoadTransactionTypes();
            }

            return this.transactionTypes;
        }

        public void AddActiveAddress(IEnumerable<AddressListViewModel> addresses)
        {
            foreach (var item in addresses)
            {
                this.addresses.RemoveAll(x => x.Address == item.Address);
                this.addresses.Add(item);
            }

            this.addresses = this.addresses
                .OrderByDescending(x => x.LastTransactionTime)
                .Take(StateService.CachedAddressesCount)
                .ToList();
        }

        public IPagedList<AddressListViewModel> GetAddressesPage(int page = 1, int pageSize = 10) =>
            this.addresses.AsQueryable().ToPagedList(page, pageSize);

        public void AddToTransactionsList(TransactionListViewModel tx)
        {
            this.transactions.Add(tx);
            this.EnsureTransactionsList();
        }

        public void AddToTransactionsList(IEnumerable<TransactionListViewModel> txs)
        {
            this.transactions.AddRange(txs);
            this.EnsureTransactionsList();
        }

        public IPagedList<TransactionListViewModel> GetTransactionsPage(int page = 1, int pageSize = 10, string type = null)
        {
            var query = this.transactions.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                var txType = Enum.Parse<TransactionType>(type);
                query = query.Where(x => x.Type == txType);
            }

            return query
                .OrderByDescending(x => x.Timestamp)
                .ToPagedList(page, pageSize);
        }

        private void EnsureTransactionsList()
        {
            if (this.transactions.Count > StateService.CachedTransactionsCount)
            {
                this.transactions = this.transactions
                    .TakeLast(StateService.CachedTransactionsCount)
                    .ToList();
            }
        }

        public void LoadTransactionTypes()
        {
            this.transactionTypes = this.db.Transactions
                 .Select(x => x.Type)
                 .GroupBy(x => x)
                 .Select(x => new ChartStatsViewModel
                 {
                     Label = x.Key.ToString(),
                     Value = x.Count()
                 })
                 .ToList();

            this.transactionTypesLastUpdate = DateTime.UtcNow;
        }

        public void LoadTransactionsMainChart()
        {
            var transactions = this.GetChart("transactions");
            transactions[UnitOfTime.Hour] = this.GetTransactionsStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Hour, EndPeriod = 36 });
            transactions[UnitOfTime.Day] = this.GetTransactionsStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Day, EndPeriod = 36 });
            transactions[UnitOfTime.Month] = this.GetTransactionsStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Month, EndPeriod = 36 });
        }

        public void LoadCreatedAddressesMainChart()
        {
            var addresses = this.GetChart("createdAddresses");
            addresses[UnitOfTime.Hour] = this.GetAddressesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Hour, EndPeriod = 36 });
            addresses[UnitOfTime.Day] = this.GetAddressesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Day, EndPeriod = 36 });
            addresses[UnitOfTime.Month] = this.GetAddressesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Month, EndPeriod = 36 });
        }

        private void LoadBlockSizesMainChart()
        {
            var blockSizes = this.GetChart("blockSizes");
            blockSizes[UnitOfTime.Hour] = this.GetBlockSizesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Hour, EndPeriod = 36 });
            blockSizes[UnitOfTime.Day] = this.GetBlockSizesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Day, EndPeriod = 36 });
            blockSizes[UnitOfTime.Month] = this.GetBlockSizesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Month, EndPeriod = 36 });
        }

        private void LoadBlockTimesMainChart()
        {
            var blockTimes = this.GetChart("blockTimes");
            blockTimes[UnitOfTime.Hour] = this.GetBlockTimesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Hour, EndPeriod = 36 });
            blockTimes[UnitOfTime.Day] = this.GetBlockTimesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Day, EndPeriod = 36 });
            blockTimes[UnitOfTime.Month] = this.GetBlockTimesStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Month, EndPeriod = 36 });
        }

        private void LoadLastActiveAddresses()
        {
            this.addresses = this.db.Addresses
                .Include(x => x.AddressesInAssetTransactions)
                .OrderByDescending(x => x.LastTransactionStamp)
                .ProjectTo<AddressListViewModel>()
                .Take(StateService.CachedAddressesCount)
                .ToList();
        }

        private void LoadLastTransactions()
        {
            this.transactions = this.db.Transactions
                .OrderByDescending(x => x.Timestamp)
                .ProjectTo<TransactionListViewModel>()
                .Take(StateService.CachedTransactionsCount)
                .ToList();
        }

        private ICollection<ChartStatsViewModel> GetBlockSizesStats(ChartFilterViewModel filter)
        {
            long latestBlockDate = this.GetLatestTimestamp();

            filter.StartStamp = latestBlockDate;
            filter.StartDate = latestBlockDate.ToUnixDate();

            var result = this.GetChartEntries(filter.UnitOfTime, ChartEntryType.BlockSizes);
            var periods = filter.GetPeriodStamps().Where(x => !result.Any(y => y.Timestamp == x));

            foreach (var endStamp in periods)
            {
                var blockQuery = this.db.Blocks
                    .Where(x => x.Timestamp <= latestBlockDate && x.Timestamp >= endStamp);
                var count = blockQuery.Count();
                var sum = count > 0 ? blockQuery.Select(x => (long)x.Size).ToList().Sum() : 0;

                result.Add(new ChartStatsViewModel
                {
                    Label = endStamp.ToUnixDate().ToString(),
                    AccumulatedValue = (decimal)sum,
                    Value = (decimal)count,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                var exists = this.db.ChartEntries.Any(
                    x =>
                        x.Timestamp == endStamp
                        && x.Type == ChartEntryType.BlockSizes
                        && x.UnitOfTime == filter.UnitOfTime);

                latestBlockDate = endStamp;

                if (exists || !endStamp.IsPeriodOver(filter.UnitOfTime))
                {
                    continue;
                }

                this.db.ChartEntries.Add(new Data.Models.ChartEntry
                {
                    UnitOfTime = filter.UnitOfTime,
                    Timestamp = endStamp,
                    Type = ChartEntryType.BlockSizes,
                    AccumulatedValue = sum,
                    Value = count
                });

                this.db.SaveChanges();
            }

            return result;
        }

        private ICollection<ChartStatsViewModel> GetBlockTimesStats(ChartFilterViewModel filter)
        {
            var latestBlockDate = this.GetLatestTimestamp();

            filter.StartDate = latestBlockDate.ToUnixDate();
            filter.StartStamp = latestBlockDate;

            var result = this.GetChartEntries(filter.UnitOfTime, ChartEntryType.BlockTimes);
            var periods = filter.GetPeriodStamps().Where(x => !result.Any(y => y.Timestamp == x));
            foreach (var endStamp in periods)
            {
                var blocksQuery = this.db.Blocks
                    .Where(x => x.Timestamp <= latestBlockDate && x.Timestamp >= endStamp);
                var count = blocksQuery.Count();
                var sum = count > 0 ? blocksQuery.Sum(x => x.TimeInSeconds) : 0;

                result.Add(new ChartStatsViewModel
                {
                    Label = endStamp.ToUnixDate().ToString(),
                    AccumulatedValue = (decimal)sum,
                    Value = (decimal)count,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                var exists = this.db.ChartEntries.Any(
                    x =>
                        x.Timestamp == endStamp
                        && x.Type == ChartEntryType.BlockTimes
                        && x.UnitOfTime == filter.UnitOfTime);

                latestBlockDate = endStamp;

                if (exists || !endStamp.IsPeriodOver(filter.UnitOfTime))
                {
                    continue;
                }

                this.db.ChartEntries.Add(new Data.Models.ChartEntry
                {
                    UnitOfTime = filter.UnitOfTime,
                    Timestamp = endStamp,
                    Type = ChartEntryType.BlockTimes,
                    AccumulatedValue = (decimal)sum,
                    Value = count
                });

                this.db.SaveChanges();
            }

            return result;
        }

        private ICollection<ChartStatsViewModel> GetAddressesStats(ChartFilterViewModel filter)
        {
            var latestBlockDate = this.GetLatestTimestamp();

            filter.StartDate = latestBlockDate.ToUnixDate();
            filter.StartStamp = latestBlockDate;

            var result = this.GetChartEntries(filter.UnitOfTime, ChartEntryType.CreatedAddresses);
            var query = this.db.Addresses
                .Where(x =>
                    x.FirstTransactionOn >= filter.GetEndPeriod()
                    && !result.Any(r => r.Timestamp == x.FirstTransactionOn.GetPeriodStart(filter.UnitOfTime).ToUnixTimestamp()));

            var newEntries = new List<ChartStatsViewModel>();
            if (filter.UnitOfTime == UnitOfTime.Hour)
            {
                newEntries = query
                    .ToList()
                    .GroupBy(x => new
                    {
                        x.FirstTransactionOn.Year,
                        x.FirstTransactionOn.Month,
                        x.FirstTransactionOn.Day,
                        x.FirstTransactionOn.Hour
                    })
                    .Select(x => new ChartStatsViewModel
                    {
                        StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, x.Key.Hour, 0, 0),
                        Timestamp = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, x.Key.Hour, 0, 0).ToUnixTimestamp(),
                        UnitOfTime = UnitOfTime.Hour,
                        Value = x.Count()
                    })
                    .OrderBy(x => x.StartDate)
                    .ToList();

                result.AddRange(newEntries);
            }
            else if (filter.UnitOfTime == UnitOfTime.Day)
            {
                newEntries = query
                    .ToList()
                    .GroupBy(x => new
                    {
                        x.FirstTransactionOn.Year,
                        x.FirstTransactionOn.Month,
                        x.FirstTransactionOn.Day
                    })
                    .Select(x => new ChartStatsViewModel
                    {
                        StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day),
                        Timestamp = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day).ToUnixTimestamp(),
                        UnitOfTime = UnitOfTime.Day,
                        Value = x.Count()
                    })
                    .OrderBy(x => x.StartDate)
                    .ToList();

                result.AddRange(newEntries);
            }
            else if (filter.UnitOfTime == UnitOfTime.Month)
            {
                newEntries = query
                    .ToList()
                    .GroupBy(x => new
                    {
                        x.FirstTransactionOn.Year,
                        x.FirstTransactionOn.Month
                    })
                    .Select(x => new ChartStatsViewModel
                    {
                        StartDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                        Timestamp = new DateTime(x.Key.Year, x.Key.Month, 1).ToUnixTimestamp(),
                        UnitOfTime = UnitOfTime.Month,
                        Value = x.Count()
                    })
                    .OrderBy(x => x.StartDate)
                    .ToList();

                result.AddRange(newEntries);
            }

            foreach (var entry in newEntries)
            {
                var exists = this.db.ChartEntries.Any(
                    x =>
                        x.Timestamp == entry.Timestamp
                        && x.Type == ChartEntryType.CreatedAddresses
                        && x.UnitOfTime == entry.UnitOfTime);

                if (exists)
                {
                    continue;
                }

                this.db.ChartEntries.Add(new Data.Models.ChartEntry
                {
                    UnitOfTime = filter.UnitOfTime,
                    Timestamp = entry.Timestamp,
                    Type = ChartEntryType.CreatedAddresses,
                    Value = entry.Value
                });

                this.db.SaveChanges();
            }

            return result;
        }

        private ICollection<ChartStatsViewModel> GetTransactionsStats(ChartFilterViewModel filter)
        {
            filter.StartDate = this.lastBlockTime.Value.ToUnixDate();
            filter.StartStamp = this.lastBlockTime.Value;

            var result = this.GetChartEntries(filter.UnitOfTime, ChartEntryType.Transactions);
            var periods = filter.GetPeriodStamps().Where(x => !result.Any(y => y.Timestamp == x)).ToArray();

            for (int i = 1; i < periods.Length; i++)
            {
                var count = this.db.Transactions.Count(x => x.Timestamp <= periods[i - 1] && x.Timestamp >= periods[i]);
                result.Add(new ChartStatsViewModel
                {
                    Value = (decimal)count,
                    StartDate = DateOrderFilter.GetDateTime(periods[i], filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                var exists = this.db.ChartEntries.Any(
                    x =>
                        x.Timestamp == periods[i]
                        && x.Type == ChartEntryType.Transactions
                        && x.UnitOfTime == filter.UnitOfTime);

                if (exists || !periods[i].IsPeriodOver(filter.UnitOfTime))
                {
                    continue;
                }

                this.db.ChartEntries.Add(new Data.Models.ChartEntry
                {
                    UnitOfTime = filter.UnitOfTime,
                    Timestamp = periods[i],
                    Type = ChartEntryType.Transactions,
                    Value = count
                });

                this.db.SaveChanges();
            }

            return result.ToList();
        }

        private void AddChartValues(string chartName, double value, DateTime time, UnitOfTime unitOfTime, double? accumulatedValue = 0)
        {
            var chart = this.GetChart(chartName);
            var lastRecord = chart[unitOfTime].OrderByDescending(x => x.StartDate).FirstOrDefault();
            if (lastRecord.StartDate.IsInSamePeriodAs(time, unitOfTime))
            {
                lastRecord.Value += (decimal)value;
            }
            else
            {
                lastRecord = new ChartStatsViewModel
                {
                    StartDate = time,
                    UnitOfTime = unitOfTime,
                    Value = (decimal)value
                };

                if (this.charts[chartName][unitOfTime].Count > 100)
                {
                    this.charts[chartName][unitOfTime] = this.charts[chartName][unitOfTime]
                        .OrderByDescending(x => x.StartDate)
                        .Take(35)
                        .ToList();
                }

                this.charts[chartName][unitOfTime].Add(lastRecord);
            }

            if (accumulatedValue != 0)
            {
                lastRecord.AccumulatedValue += (decimal)accumulatedValue.Value;
            }
        }

        private ICollection<ChartStatsViewModel> GetChartPer(string chart, UnitOfTime unitOfTime) =>
            this.GetChart(chart)[unitOfTime];

        private Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>> GetChart(string chart)
        {
            if (!this.charts.ContainsKey(chart))
            {
                this.charts.Add(chart, new Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>>
                {
                    { UnitOfTime.Day, new List<ChartStatsViewModel>() },
                    { UnitOfTime.Hour, new List<ChartStatsViewModel>() },
                    { UnitOfTime.Month, new List<ChartStatsViewModel>() }
                });
            }

            return this.charts[chart];
        }

        private long GetLatestTimestamp()
        {
            if (this.lastBlockTime == null)
            {
                this.lastBlockTime = this.db.Blocks
                    .Select(x => x.Timestamp)
                    .OrderByDescending(x => x)
                    .First();
            }

            return this.lastBlockTime.Value;
        }
    }
}
