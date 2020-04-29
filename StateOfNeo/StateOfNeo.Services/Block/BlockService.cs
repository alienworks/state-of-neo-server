using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Helpers.Filters;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Chart;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using X.PagedList;

namespace StateOfNeo.Services.Block
{
    public class BlockService : FilterService, IBlockService
    {
        public BlockService(StateOfNeoContext db) : base(db)
        {
        }

        public T Find<T>(string hash) =>
            this.db.Blocks
                .AsNoTracking()
                .Where(x => x.Hash == hash)
                .ProjectTo<T>()
                .FirstOrDefault();

        public T Find<T>(int height) =>
            this.db.Blocks
                .AsNoTracking()
                .Where(x => x.Height == height)
                .ProjectTo<T>()
                .FirstOrDefault();

        public int GetHeight(string hash) =>
            this.db.Blocks
                .AsNoTracking()
                .Where(x => x.Hash == hash)
                .Select(x => x.Height)
                .FirstOrDefault();

        private long GetLatestTimestamp()
        {
            return this.db.Blocks
                .AsNoTracking()
                .Select(x => x.Timestamp)
                .OrderByDescending(x => x)
                .First();
        }

        public decimal GetAvgTxCountPerBlock()
        {
            var txs = this.db.Transactions.AsNoTracking().Count();
            var blocks = this.db.Blocks.AsNoTracking().Count();

            return txs / blocks;
        }

        public double GetAvgBlockTime() => this.db.Blocks.AsNoTracking().Average(x => x.TimeInSeconds);

        public double GetAvgBlockSize() => this.db.Blocks.AsNoTracking().Average(x => x.Size);

        public string NextBlockHash(int height) => this.db.Blocks
            .AsNoTracking()
            .Where(x => x.Height == height + 1)
            .Select(x => x.Hash)
            .FirstOrDefault();

        public IEnumerable<ChartStatsViewModel> GetBlockSizeStats(ChartFilterViewModel filter)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long latestBlockDate = this.GetLatestTimestamp();
            filter.StartDate = latestBlockDate.ToUnixDate();

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            foreach (var endStamp in filter.GetPeriodStamps())
            {
                var avgSize = this.db.Blocks
                    .AsNoTracking()
                    .Where(x => x.Timestamp <= latestBlockDate && x.Timestamp >= endStamp)
                    .Average(x => x.Size);

                result.Add(new ChartStatsViewModel
                {
                    Value = (decimal)avgSize,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                latestBlockDate = endStamp;
            }

            stopwatch.Stop();
            Log.Information("GetBlockSizeStats time - " + stopwatch.ElapsedMilliseconds);
            return result;
        }

        public IEnumerable<ChartStatsViewModel> GetBlockTimeStats(ChartFilterViewModel filter)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var latestBlockDate = this.GetLatestTimestamp();
            filter.StartDate = latestBlockDate.ToUnixDate();

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            var periods = filter.GetPeriodStamps();
            foreach (var endStamp in periods)
            {
                var avgInterval = this.db.Blocks
                    .AsNoTracking()
                    .Where(x => x.Timestamp <= latestBlockDate && x.Timestamp >= endStamp)
                    .Average(x => x.TimeInSeconds);

                result.Add(new ChartStatsViewModel
                {
                    Value = (decimal)avgInterval,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                latestBlockDate = endStamp;
            }

            stopwatch.Stop();
            Log.Information("GetBlockTimeStats time - " + stopwatch.ElapsedMilliseconds);
            return result;
        }
    }
}