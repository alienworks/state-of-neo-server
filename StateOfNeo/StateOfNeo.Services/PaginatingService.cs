using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using X.PagedList;
using StateOfNeo.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StateOfNeo.Services
{
    public class PaginatingService : IPaginatingService
    {
        protected readonly StateOfNeoContext db;

        public PaginatingService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public async Task<IPagedList<TDestination>> GetPage<TFrom, TDestination>(
            int page = 1, 
            int pageSize = 10, 
            Expression<Func<TFrom, object>> order = null,
            Expression<Func<TFrom, bool>> filter = null) where TFrom : class
        {
            var query = this.db.Set<TFrom>().AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (order != null)
            {
                query = query.OrderByDescending(order);
            }
            
            return await query
                .ProjectTo<TDestination>()
                .ToPagedListAsync(page, pageSize);
        }
    }
}
