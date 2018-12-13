using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Common.Enums;
using StateOfNeo.Data;
using StateOfNeo.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StateOfNeo.Services
{
    public class MainStatsState : IMainStatsState
    {
        private HeaderStatsViewModel headerStats;

        private long? totalTxCount;
        private decimal? totalClaimedTx;

        private int? totalAddressCount;
        private int? totalAssetsCount;

        private double? totalBlocksTimes;
        private int? blockHeight;
        private long? totalBlocksSizes;
        
        private readonly StateOfNeoContext db;

        public MainStatsState(IOptions<DbSettings> dbOptions)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            this.db = StateOfNeoContext.Create(dbOptions.Value.DefaultConnection);

            this.GetHeaderStats();
            this.GetTotalTxCount();
            this.GetTotalAddressCount();
            this.GetTotalAssetsCount();
            this.GetTotalClaimed();
            this.GetTotalBlocksCount();
            this.GetTotalBlocksTimesCount();
            this.GetTotalBlocksSizesCount();

            this.db.Dispose();

            stopwatch.Stop();
            Log.Information($"{nameof(MainStatsState)} initialization {stopwatch.ElapsedMilliseconds} ms");
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
            if (this.totalClaimedTx == null)
            {
                this.totalClaimedTx = this.db.Transactions
                    .Include(x => x.GlobalOutgoingAssets).ThenInclude(x => x.Asset)
                    .Where(x => x.Type == Neo.Network.P2P.Payloads.TransactionType.ClaimTransaction)
                    .SelectMany(x => x.GlobalOutgoingAssets.Where(a => a.AssetType == AssetType.GAS))
                    .Sum(x => x.Amount);
            }

            return this.totalClaimedTx.Value;
        }

        public void AddTotalClaimed(decimal amount)
        {
            this.totalClaimedTx = this.GetTotalClaimed() + amount;
        }

        public int GetTotalBlocksCount()
        {
            if (this.blockHeight == null) this.blockHeight = this.db.Blocks.Count();
            return this.blockHeight.Value;
        }

        public void AddTotalBlocksCount(int count)
        {
            this.blockHeight = this.GetTotalBlocksCount() + count;
        }

        public double GetTotalBlocksTimesCount()
        {
            if (this.totalBlocksTimes == null) this.totalBlocksTimes = this.db.Blocks.Sum(x => x.TimeInSeconds);
            return this.totalBlocksTimes.Value;
        }

        public void AddToTotalBlocksTimesCount(double value)
        {
            this.totalBlocksTimes = this.GetTotalBlocksTimesCount() + value;
        }

        public long GetTotalBlocksSizesCount()
        {
            if (this.totalBlocksSizes == null) this.totalBlocksSizes = this.db.Blocks.Select(x => (long)x.Size).ToList().Sum();
            return this.totalBlocksSizes.Value;
        }

        public void AddToTotalBlocksSizesCount(int value)
        {
            this.totalBlocksSizes = this.GetTotalBlocksSizesCount() + value;
        }
    }
}
