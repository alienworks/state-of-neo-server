using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Helpers.Filters;
using StateOfNeo.Common.Helpers.Models;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;
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

        public int AverageBlockSize(UnitOfTime timePeriod)
        {
            var result = 0;
            if (timePeriod == UnitOfTime.Hour)
            {
                result = this.db.Blocks.Count() / this.db
                    .Blocks
                    .GroupBy(x => new
                    {
                        x.Timestamp.ToUnixDate().Year,
                        x.Timestamp.ToUnixDate().Month,
                        x.Timestamp.ToUnixDate().Day,
                        x.Timestamp.ToUnixDate().Hour
                    })
                    .Count();
            }
            else if (timePeriod == UnitOfTime.Day)
            {
                result = this.db.Blocks.Count() / this.db
                    .Blocks
                    .GroupBy(x => new
                    {
                        x.Timestamp.ToUnixDate().Year,
                        x.Timestamp.ToUnixDate().Month,
                        x.Timestamp.ToUnixDate().Day
                    })
                    .Count();
            }
            else if (timePeriod == UnitOfTime.Month)
            {
                result = this.db.Blocks.Count() / this.db
                    .Blocks
                    .GroupBy(x => new
                    {
                        x.Timestamp.ToUnixDate().Year,
                        x.Timestamp.ToUnixDate().Month
                    })
                    .Count();
            }

            return result;
        }

        public IEnumerable<ChartStatsViewModel> GetBlockSizeStats(ChartFilterViewModel filter)
        {
            return this.Filter<Data.Models.Block>(filter,
                x => new ValueExtractionModel
                {
                    Size = x.Size,
                    Timestamp = x.Timestamp
                });
        }

        public IEnumerable<ChartStatsViewModel> GetBlockTimeStats(ChartFilterViewModel filter)
        {
            return this.Filter<Data.Models.Block>(filter,
                x => new ValueExtractionModel
                {
                    Size = (decimal)(x.Timestamp.ToUnixDate() - x.PreviousBlock.Timestamp.ToUnixDate()).TotalSeconds,
                    Timestamp = x.Timestamp
                },
                x => x.PreviousBlock != null);
        }

    }
}
