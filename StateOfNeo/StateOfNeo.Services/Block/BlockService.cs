using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Helpers.Filters;
using StateOfNeo.Common.Helpers.Models;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using X.PagedList;

namespace StateOfNeo.Services.Block
{
    public class BlockService : FilterService, IBlockService
    {
        public BlockService(StateOfNeoContext db) : base(db) { }

        public T Find<T>(string hash) =>
            this.db.Blocks
                .Where(x => x.Hash == hash)
                .ProjectTo<T>()
                .FirstOrDefault();

        public T Find<T>(int height) =>
            this.db.Blocks
                 .Where(x => x.Height == height)
                 .ProjectTo<T>()
                 .FirstOrDefault();

        public IEnumerable<ChartStatsViewModel> GetBlockSizeStats(ChartFilterViewModel filter)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long latestBlockDate = this.GetLatestTimestamp();

            filter.StartStamp = latestBlockDate;
            filter.StartDate = latestBlockDate.ToUnixDate();

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            foreach (var endStamp in filter.GetPeriodStamps())
            {
                var avg = this.db.Blocks
                    .Where(x => x.Timestamp <= latestBlockDate && x.Timestamp >= endStamp)
                    .Average(x => x.Size);

                result.Add(new ChartStatsViewModel
                {
                    Value = (decimal)avg,
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
            filter.StartStamp = latestBlockDate;

            List<ChartStatsViewModel> result = new List<ChartStatsViewModel>();
            var periods = filter.GetPeriodStamps();
            foreach (var endStamp in periods)
            {
                var avg = this.db.Blocks
                    .Where(x => x.Timestamp <= latestBlockDate && x.Timestamp >= endStamp)
                    .Average(x => x.TimeInSeconds);

                result.Add(new ChartStatsViewModel
                {
                    Value = (decimal)avg,
                    StartDate = DateOrderFilter.GetDateTime(endStamp, filter.UnitOfTime),
                    UnitOfTime = filter.UnitOfTime
                });

                latestBlockDate = endStamp;
            }

            stopwatch.Stop();
            Log.Information("GetBlockTimeStats time - " + stopwatch.ElapsedMilliseconds);
            return result;
        }

        private long GetLatestTimestamp()
        {
            return this.db.Blocks
                .Select(x => x.Timestamp)
                .OrderByDescending(x => x)
                .First();
        }

        public decimal GetAvgTxPerBlock()
        {
            var txs = this.db.Transactions.Count();
            var blocks = this.db.Blocks.Count();

            return txs / blocks;
        }

        public double GetAvgBlockTime() => this.db.Blocks.Average(x => x.TimeInSeconds);

        public double GetAvgBlockSize() => this.db.Blocks.Average(x => x.Size);
    }
}
