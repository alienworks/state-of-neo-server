using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public interface IPaginatingService
    {
        Task<IPagedList<TDestination>> GetPage<TFrom, TDestination>(int page = 1, int pageSize = 10) 
            where TFrom : class;
    }
}
