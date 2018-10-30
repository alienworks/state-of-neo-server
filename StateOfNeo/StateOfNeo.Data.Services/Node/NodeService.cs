using AutoMapper.QueryableExtensions;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Data.Services
{
    public class NodeService : INodeService
    {
        private readonly StateOfNeoContext ctx;

        public NodeService(StateOfNeoContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<IPagedList<T>> GetPage<T>(int page = 1, int pageSize = 10)
        {
            var result = await this.ctx.Nodes
                .ProjectTo<T>()
                .ToPagedListAsync(page, pageSize);

            return result;
        }
    }
}
