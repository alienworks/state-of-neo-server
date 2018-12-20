using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Common.Enums;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StateOfNeo.Services
{
    public class MainStatsState : IMainStatsState
    {
        public TotalStats TotalStats { get; set; }

        private readonly StateOfNeoContext db;
        private HeaderStatsViewModel headerStats;

        public MainStatsState(IOptions<DbSettings> dbOptions)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            this.db = StateOfNeoContext.Create(dbOptions.Value.DefaultConnection);
            this.TotalStats = this.db.TotalStats.FirstOrDefault();

            this.GetHeaderStats();

            if (this.TotalStats == null)
            {
                this.TotalStats = new TotalStats
                {
                    BlockCount = this.headerStats.Height,
                    Timestamp = this.headerStats.Timestamp
                };

                this.db.TotalStats.Add(this.TotalStats);
                this.db.SaveChanges();
            }

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
            if (this.TotalStats.TransactionsCount == null) this.TotalStats.TransactionsCount = this.db.Transactions.Count();
            return this.TotalStats.TransactionsCount.Value;
        }

        public void AddToTotalTxCount(int count)
        {
            this.TotalStats.TransactionsCount = this.GetTotalTxCount() + count;
        }

        public int GetTotalAddressCount()
        {
            if (this.TotalStats.AddressCount == null) this.TotalStats.AddressCount = this.db.Addresses.Count();
            return this.TotalStats.AddressCount.Value;
        }

        public void AddTotalAddressCount(int count)
        {
            this.TotalStats.AddressCount = this.GetTotalAddressCount() + count;
        }

        public int GetTotalAssetsCount()
        {
            if (this.TotalStats.AssetsCount == null) this.TotalStats.AssetsCount = this.db.Assets.Count();
            return this.TotalStats.AssetsCount.Value;
        }

        public void AddTotalAssetsCount(int count)
        {
            this.TotalStats.AssetsCount = this.GetTotalAssetsCount() + count;
        }

        public decimal GetTotalClaimed()
        {
            if (this.TotalStats.ClaimedGas == null)
            {
                this.TotalStats.ClaimedGas = this.db.Transactions
                    .Include(x => x.GlobalOutgoingAssets).ThenInclude(x => x.Asset)
                    .Where(x => x.Type == Neo.Network.P2P.Payloads.TransactionType.ClaimTransaction)
                    .SelectMany(x => x.GlobalOutgoingAssets.Where(a => a.AssetType == AssetType.GAS))
                    .Sum(x => x.Amount);
            }

            return this.TotalStats.ClaimedGas.Value;
        }

        public void AddTotalClaimed(decimal amount)
        {
            this.TotalStats.ClaimedGas = this.GetTotalClaimed() + amount;
        }

        public int GetTotalBlocksCount()
        {
            if (this.TotalStats.BlockCount == null) this.TotalStats.BlockCount = this.db.Blocks.Count();
            return this.TotalStats.BlockCount.Value;
        }

        public void AddTotalBlocksCount(int count)
        {
            this.TotalStats.BlockCount = this.GetTotalBlocksCount() + count;
        }

        public decimal GetTotalBlocksTimesCount()
        {
            if (this.TotalStats.BlocksTimes == null) this.TotalStats.BlocksTimes = (decimal)this.db.Blocks.Sum(x => x.TimeInSeconds);
            return this.TotalStats.BlocksTimes.Value;
        }

        public void AddToTotalBlocksTimesCount(decimal value)
        {
            this.TotalStats.BlocksTimes = this.GetTotalBlocksTimesCount() + value;
        }

        public long GetTotalBlocksSizesCount()
        {
            if (this.TotalStats.BlocksSizes == null) this.TotalStats.BlocksSizes = this.db.Blocks.Select(x => (long)x.Size).ToList().Sum();
            return this.TotalStats.BlocksSizes.Value;
        }

        public void AddToTotalBlocksSizesCount(int value)
        {
            this.TotalStats.BlocksSizes = this.GetTotalBlocksSizesCount() + value;
        }
    }
}
