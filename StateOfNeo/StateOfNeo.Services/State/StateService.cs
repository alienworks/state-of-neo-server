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

namespace StateOfNeo.Services
{
    public class StateService : IStateService
    {
        private Dictionary<string, Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>>> charts = new Dictionary<string, Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>>>();
        private ICollection<ChartStatsViewModel> transactionTypes = new List<ChartStatsViewModel>();
        private DateTime? transactionTypesLastUpdate;
        private readonly StateOfNeoContext db;

        public IMainStatsState MainStats { get; }
        public IContractsState Contracts { get; }

        public StateService(IOptions<DbSettings> dbOptions, IMainStatsState mainStats)
        {
            this.MainStats = mainStats;

            Stopwatch stopwatch = Stopwatch.StartNew();
            this.db = StateOfNeoContext.Create(dbOptions.Value.DefaultConnection);

            this.Contracts = new ContractsState();

            this.LoadTransactionTypes();
            this.LoadTransactionsMainChart();
            this.LoadCreatedAddressesMainChart();
            this.LoadBlockTimesMainChart();
            this.LoadBlockSizesMainChart();

            stopwatch.Stop();
            Log.Information($"{nameof(StateService)} initialization {stopwatch.ElapsedMilliseconds} ms");
        }

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

        private ICollection<ChartStatsViewModel> GetBlockSizesStats(ChartFilterViewModel filter)
        {
            long latestBlockDate = this.GetLatestTimestamp();

            filter.StartStamp = latestBlockDate;
            filter.StartDate = latestBlockDate.ToUnixDate();

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            foreach (var endStamp in filter.GetPeriodStamps())
            {
                var blockQuery = this.db.Blocks
                    .Where(x => x.Timestamp <= latestBlockDate && x.Timestamp >= endStamp);
                var count = blockQuery.Count();
                var sum = count > 0 ? blockQuery.Sum(x => x.Size) : 0;

                result.Add(new ChartStatsViewModel
                {
                    Label = endStamp.ToUnixDate().ToString(),
                    AccumulatedValue = (decimal)sum,
                    Value = (decimal)count,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                latestBlockDate = endStamp;
            }

            return result;
        }

        private ICollection<ChartStatsViewModel> GetBlockTimesStats(ChartFilterViewModel filter)
        {
            var latestBlockDate = this.GetLatestTimestamp();

            filter.StartDate = latestBlockDate.ToUnixDate();
            filter.StartStamp = latestBlockDate;

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            var periods = filter.GetPeriodStamps();
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

                latestBlockDate = endStamp;
            }

            return result;
        }

        private ICollection<ChartStatsViewModel> GetAddressesStats(ChartFilterViewModel filter)
        {
            if (filter.StartDate == null)
            {
                filter.StartDate = this.db.Addresses
                    .OrderByDescending(x => x.FirstTransactionOn)
                    .Select(x => x.FirstTransactionOn)
                    .FirstOrDefault();
            }

            var result = new List<ChartStatsViewModel>();
            var query = this.db.Addresses.Where(x => x.FirstTransactionOn >= filter.GetEndPeriod());

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

        private ICollection<ChartStatsViewModel> GetTransactionsStats(ChartFilterViewModel filter)
        {
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
                    this.charts[chartName][unitOfTime] = this.charts[chartName][unitOfTime].OrderByDescending(x => x.StartDate).Take(35).ToList();
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
            var block = this.db.Blocks
                .OrderByDescending(x => x.Timestamp)
                .First();
            return block.Timestamp;
        }
    }
}
