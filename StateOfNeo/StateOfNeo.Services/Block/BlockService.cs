using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
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
        
        public int AverageBlockSize(TimePeriod timePeriod)
        {
            var result = 0;
            if (timePeriod == TimePeriod.Hour)
            {
                result = this.db.Blocks.Count() / this.db
                    .Blocks
                    .GroupBy(x => new
                    {
                        x.Timestamp.ToCurrentDate().Year,
                        x.Timestamp.ToCurrentDate().Month,
                        x.Timestamp.ToCurrentDate().Day,
                        x.Timestamp.ToCurrentDate().Hour
                    })
                    .Count();
            }
            else if (timePeriod == TimePeriod.Day)
            {
                result = this.db.Blocks.Count() / this.db
                    .Blocks
                    .GroupBy(x => new
                    {
                        x.Timestamp.ToCurrentDate().Year,
                        x.Timestamp.ToCurrentDate().Month,
                        x.Timestamp.ToCurrentDate().Day
                    })
                    .Count();
            }
            else if (timePeriod == TimePeriod.Month)
            {
                result = this.db.Blocks.Count() / this.db
                    .Blocks
                    .GroupBy(x => new
                    {
                        x.Timestamp.ToCurrentDate().Year,
                        x.Timestamp.ToCurrentDate().Month
                    })
                    .Count();
            }

            return result;
        }        
    }
}
