using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Chart;
using X.PagedList;

namespace StateOfNeo.Services.Block
{
    public class BlockService : IBlockService
    {
        private readonly StateOfNeoContext db;

        public BlockService(StateOfNeoContext db)
        {
            this.db = db;
        }

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
            var query = this.db.Blocks.AsQueryable();
            var result = new List<ChartStatsViewModel>();

            if (filter.StartDate != null)
            {
                query = query.Where(x => x.Timestamp.ToUnixDate() >= filter.StartDate);
            }

            if (filter.EndDate != null)
            {
                query = query.Where(x => x.Timestamp.ToUnixDate() <= filter.EndDate);
            }

            if (filter.UnitOfTime == UnitOfTime.Hour)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.Timestamp.ToUnixDate().Year,
                    x.Timestamp.ToUnixDate().Month,
                    x.Timestamp.ToUnixDate().Day,
                    x.Timestamp.ToUnixDate().Hour
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, x.Key.Hour, 0, 0),
                    UnitOfTime = UnitOfTime.Hour,
                    Value = x.Sum(z => z.Size) / x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Day)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.Timestamp.ToUnixDate().Year,
                    x.Timestamp.ToUnixDate().Month,
                    x.Timestamp.ToUnixDate().Day
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day),
                    UnitOfTime = UnitOfTime.Day,
                    Value = x.Sum(z => z.Size) / x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Month)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.Timestamp.ToUnixDate().Year,
                    x.Timestamp.ToUnixDate().Month
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                    UnitOfTime = UnitOfTime.Month,
                    Value = x.Sum(z => z.Size) / x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }

            return result;
        }

        public IEnumerable<ChartStatsViewModel> GetBlockTimeStats(ChartFilterViewModel filter)
        {
            var query = this.db.Blocks.Include(x => x.PreviousBlock).AsQueryable();
            var result = new List<ChartStatsViewModel>();

            if (filter.StartDate != null)
            {
                query = query.Where(x => x.Timestamp.ToUnixDate() >= filter.StartDate);
            }

            if (filter.EndDate != null)
            {
                query = query.Where(x => x.Timestamp.ToUnixDate() <= filter.EndDate);
            }

            query = query.Where(x => x.PreviousBlock != null);
            if (filter.UnitOfTime == UnitOfTime.Hour)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.Timestamp.ToUnixDate().Year,
                    x.Timestamp.ToUnixDate().Month,
                    x.Timestamp.ToUnixDate().Day,
                    x.Timestamp.ToUnixDate().Hour
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, x.Key.Hour, 0, 0),
                    UnitOfTime = UnitOfTime.Hour,
                    Value = x.Where(z => z.PreviousBlock != null).Sum(z => (decimal)(z.Timestamp.ToUnixDate() - z.PreviousBlock.Timestamp.ToUnixDate()).TotalSeconds) / x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Day)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.Timestamp.ToUnixDate().Year,
                    x.Timestamp.ToUnixDate().Month,
                    x.Timestamp.ToUnixDate().Day
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day),
                    UnitOfTime = UnitOfTime.Day,
                    Value = x.Where(z => z.PreviousBlock != null).Sum(z => (decimal)(z.Timestamp.ToUnixDate() - z.PreviousBlock.Timestamp.ToUnixDate()).TotalSeconds) / x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }
            else if (filter.UnitOfTime == UnitOfTime.Month)
            {
                result = query.ToList().GroupBy(x => new
                {
                    x.Timestamp.ToUnixDate().Year,
                    x.Timestamp.ToUnixDate().Month
                })
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                    UnitOfTime = UnitOfTime.Month,
                    Value = x.Where(z => z.PreviousBlock != null).Sum(z => (decimal)(z.Timestamp.ToUnixDate() - z.PreviousBlock.Timestamp.ToUnixDate()).TotalSeconds) / x.Count()
                })
                .OrderBy(x => x.StartDate)
                .ToList();
            }

            return result;
        }
    }
}
