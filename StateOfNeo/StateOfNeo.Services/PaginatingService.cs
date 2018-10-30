using AutoMapper.QueryableExtensions;
using StateOfNeo.Data;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public class PaginatingService : IPaginatingService
    {
        protected readonly StateOfNeoContext db;

        public PaginatingService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public async Task<IPagedList<TDestination>> GetPage<TFrom, TDestination>(int page = 1, int pageSize = 10)
            where TFrom : class => 
                await this.db.Set<TFrom>()
                    .ProjectTo<TDestination>()
                    .ToPagedListAsync(page, pageSize);
    }
}
