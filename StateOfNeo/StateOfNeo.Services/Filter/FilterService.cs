using Microsoft.EntityFrameworkCore;
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

namespace StateOfNeo.Services
{
    public class FilterService
    {
        internal readonly StateOfNeoContext db;

        public FilterService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public IEnumerable<ChartStatsViewModel> Filter<T>(
            ChartFilterViewModel chartFilter,
            Expression<Func<T, ValueExtractionModel>> projection = null,
            Expression<Func<T, bool>> queryFilter = null)
                where T : StampedEntity
        {
            this.ConfirmStartDateValue<T>(chartFilter);

            var query = this.db.Set<T>().AsQueryable();

            if (queryFilter != null)
            {
                query = query.Where(queryFilter);
            }

            var filteredQuery = query.Select(projection ?? (x => new ValueExtractionModel
            {
                Size = 1,
                Timestamp = x.Timestamp
            }));

            var result = filteredQuery
                .GroupBy(x => DateOrderFilter.GetGroupBy(x.Timestamp, chartFilter.UnitOfTime))
                .Select(x => new ChartStatsViewModel
                {
                    StartDate = DateOrderFilter.GetDateTime(x.First().Timestamp, chartFilter.UnitOfTime),
                    UnitOfTime = chartFilter.UnitOfTime,
                    Value = projection == null ? x.Count() : x.Sum(z => z.Size) / x.Count()
                })
                .OrderBy(x => x.StartDate)
                .Take(chartFilter.EndPeriod)
                .ToList();

            return result;
        }

        private void ConfirmStartDateValue<T>(ChartFilterViewModel filter)
            where T : StampedEntity
        {
            if (filter.StartDate == null)
            {
                var latestDbBlockTime = this.db.Set<T>()
                    .OrderByDescending(x => x.Timestamp)
                    .Select(x => x.Timestamp.ToUnixDate())
                    .FirstOrDefault();

                filter.StartDate = latestDbBlockTime;
            }
        }
    }
}
