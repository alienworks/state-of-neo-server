using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public interface IPaginatingService
    {
        Task<IPagedList<TDestination>> GetPage<TFrom, TDestination>(
            int page = 1, 
            int pageSize = 10, 
            Expression<Func<TFrom, object>> order = null,
            Expression<Func<TFrom, object>> includes = null,
            Expression<Func<TFrom, bool>> filter = null) where TFrom : class;
    }
}
