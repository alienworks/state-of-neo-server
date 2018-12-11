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

namespace StateOfNeo.Services
{
    public class StateService : IStateService
    {
        private Dictionary<string, Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>>> charts = new Dictionary<string, Dictionary<UnitOfTime, ICollection<ChartStatsViewModel>>>();
        private ICollection<ChartStatsViewModel> transactionTypes = new List<ChartStatsViewModel>();
        private DateTime? transactionTypesLastUpdate;
        private readonly StateOfNeoContext db;

        private HeaderStatsViewModel headerStats;
        private long? totalTxCount;
        private int? totalAddressCount;
        private int? totalAssetsCount;
        private decimal? totalClaimed;

        private IDictionary<string, List<NotificationHubViewModel>> contractsNotifications;

        public StateService(IOptions<DbSettings> dbOptions)
        {
            this.db = StateOfNeoContext.Create(dbOptions.Value.DefaultConnection);

            this.GetHeaderStats();
            this.GetTotalTxCount();
            this.GetTotalAddressCount();
            this.GetTotalAssetsCount();
            this.GetTotalClaimed();

            this.contractsNotifications = new Dictionary<string, List<NotificationHubViewModel>>();
            this.UpdateTransactionTypes();
            this.LoadTransactionsMainChart();
        }

        public HeaderStatsViewModel GetHeaderStats()
        {
            if (this.headerStats == null)
            {
                this.headerStats = this.db.Blocks
                    .OrderByDescending(x => x.Height)
                    .ProjectTo<HeaderStatsViewModel>()
                    .FirstOrDefault();
            }

            return this.headerStats;
        }

        public void SetHeaderStats(HeaderStatsViewModel newValue)
        {
            this.headerStats = newValue;
        }

        public long GetTotalTxCount()
        {
            if (this.totalTxCount == null) this.totalTxCount = this.db.Transactions.Count();
            return this.totalTxCount.Value;
        }

        public void AddToTotalTxCount(int count)
        {
            this.totalTxCount = this.GetTotalTxCount() + count;
        }

        public int GetTotalAddressCount()
        {
            if (this.totalAddressCount == null) this.totalAddressCount = this.db.Addresses.Count();
            return this.totalAddressCount.Value;
        }

        public void AddTotalAddressCount(int count)
        {
            this.totalAddressCount = this.GetTotalAddressCount() + count;
        }

        public int GetTotalAssetsCount()
        {
            if (this.totalAssetsCount == null) this.totalAssetsCount = this.db.Assets.Count();
            return this.totalAssetsCount.Value;
        }

        public void AddTotalAssetsCount(int count)
        {
            this.totalAssetsCount = this.GetTotalAssetsCount() + count;
        }

        public decimal GetTotalClaimed()
        {
            if (this.totalClaimed == null)
            {
                this.totalClaimed = this.db.Transactions
                    .Include(x => x.GlobalOutgoingAssets).ThenInclude(x => x.Asset)
                    .Where(x => x.Type == Neo.Network.P2P.Payloads.TransactionType.ClaimTransaction)
                    .SelectMany(x => x.GlobalOutgoingAssets.Where(a => a.AssetType == AssetType.GAS))
                    .Sum(x => x.Amount);
            }

            return this.totalClaimed.Value;
        }

        public ICollection<ChartStatsViewModel> GetTransactionsChart(UnitOfTime unitOfTime, int count) =>
            this.GetChart("transactions")[unitOfTime].Take(count).ToList();

        public void AddTotalClaimed(decimal amount)
        {
            this.totalClaimed = this.GetTotalClaimed() + amount;
        }

        public IEnumerable<NotificationHubViewModel> GetNotificationsForContract(string hash)
        {
            if (!this.contractsNotifications.ContainsKey(hash))
            {
                return null;
            }

            return this.contractsNotifications[hash];
        }

        public void SetOrAddNotificationsForContract(string hash, long timestamp, string[] values)
        {
            var newValue = new NotificationHubViewModel(timestamp, hash, values);
            if (!this.contractsNotifications.ContainsKey(hash))
            {
                this.contractsNotifications.Add(hash, new List<NotificationHubViewModel> { newValue });
            }
            else
            {
                this.contractsNotifications[hash].Add(newValue);

                if (this.contractsNotifications[hash].Count > NotificationConstants.MaxNotificationCount)
                {
                    this.contractsNotifications[hash] =
                        this.contractsNotifications[hash].Take(NotificationConstants.MaxNotificationCount).ToList();
                }

                if (hash != NotificationConstants.AllNotificationsKey)
                {
                    this.SetOrAddNotificationsForContract(NotificationConstants.AllNotificationsKey, timestamp,  values);
                }
            }
        }

        public void AddTransactions(int count, DateTime time)
        {
            this.AddTransactionsForPeriod(count, time, UnitOfTime.Hour);
            this.AddTransactionsForPeriod(count, time, UnitOfTime.Day);
            this.AddTransactionsForPeriod(count, time, UnitOfTime.Month);
        }

        public IEnumerable<ChartStatsViewModel> GetTransactionsPer(UnitOfTime unitOfTime, int count) =>
            this.charts["transactions"][unitOfTime].OrderByDescending(x => x.StartDate).Take(count);

        public IEnumerable<ChartStatsViewModel> GetTransactionTypes()
        {
            if (this.transactionTypesLastUpdate == null 
                || this.transactionTypesLastUpdate.Value.AddHours(6) <= DateTime.UtcNow)
            {
                this.UpdateTransactionTypes();
            }

            return this.transactionTypes;
        }

        public void UpdateTransactionTypes()
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
            transactions[UnitOfTime.Hour] = this.GetStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Hour, EndPeriod = 36 });
            transactions[UnitOfTime.Day] = this.GetStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Day, EndPeriod = 36 });
            transactions[UnitOfTime.Month] = this.GetStats(new ChartFilterViewModel { UnitOfTime = UnitOfTime.Month, EndPeriod = 36 });
        }

        public ICollection<ChartStatsViewModel> GetStats(ChartFilterViewModel filter)
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

        private void AddTransactionsForPeriod(int count, DateTime time, UnitOfTime unitOfTime)
        {
            var transactions = this.GetChart("transactions");
            var lastRecord = transactions[unitOfTime].OrderByDescending(x => x.StartDate).FirstOrDefault();
            if (lastRecord.StartDate.IsInSamePeriodAs(time, unitOfTime))
            {
                lastRecord.Value += count;
            }
            else
            {
                lastRecord = new ChartStatsViewModel
                {
                    StartDate = time,
                    UnitOfTime = unitOfTime,
                    Value = count
                };

                if (this.charts["transactions"][unitOfTime].Count > 100)
                {
                    this.charts["transactions"][unitOfTime] = this.charts["transactions"][unitOfTime].OrderByDescending(x => x.StartDate).Take(35).ToList();
                }

                this.charts["transactions"][unitOfTime].Add(lastRecord);
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
    }
}
