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

        public IEnumerable<ChartStatsViewModel> FilterCount<T>(ChartFilterViewModel filterModel,
            Expression<Func<T, bool>> filter = null)
            where T : StampedEntity
        {
            var result = new List<ChartStatsViewModel>();

            ConfirmStartDateValue<T>(filterModel);
            var query = this.db.Set<T>()
                .Where(x => x.Timestamp.ToUnixDate() >= filterModel.GetEndPeriod());

            if (filter != null)
            {
                query = query.Where(filter);
            }

            result = query
               .GroupBy(x => DateOrderFilter.GetGroupBy(x.Timestamp, filterModel.UnitOfTime))
               .Select(x => new ChartStatsViewModel
               {
                   StartDate = DateOrderFilter.GetDateTime(x.First().Timestamp, filterModel.UnitOfTime),
                   UnitOfTime = filterModel.UnitOfTime,
                   Value = x.Count()
               })
               .OrderBy(x => x.StartDate)
               .ToList();

            return result;
        }

        public IEnumerable<ChartStatsViewModel> Filter<T>(ChartFilterViewModel filterModel,
            Expression<Func<T, ValueExtractionModel>> value = null,
            Expression<Func<T, bool>> filter = null)
            where T : StampedEntity
        {
            var result = new List<ChartStatsViewModel>();

            ConfirmStartDateValue<T>(filterModel);
            var query = this.db.Set<T>().AsQueryable();
                //.Where(x => x.Timestamp.ToUnixDate() >= filterModel.GetEndPeriod());

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var filteredQuery = query.Select(value ?? (x => new ValueExtractionModel
            {
                Size = 1,
                Timestamp = x.Timestamp
            }));

            result = filteredQuery
               .GroupBy(x => DateOrderFilter.GetGroupBy(x.Timestamp, filterModel.UnitOfTime))
               .Select(x => new ChartStatsViewModel
               {
                   StartDate = DateOrderFilter.GetDateTime(x.First().Timestamp, filterModel.UnitOfTime),
                   UnitOfTime = filterModel.UnitOfTime,
                   Value = value == null ? x.Count() : x.Sum(z => z.Size) / x.Count()
               })
               .OrderBy(x => x.StartDate)
               .Take(filterModel.EndPeriod)
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
