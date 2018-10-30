using AutoMapper.QueryableExtensions;
using StateOfNeo.Data;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
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
                .Where(x => x.SuccessUrl != null)
                .ProjectTo<T>()
                .ToPagedListAsync(page, pageSize);

            return result;
        }
    }
}
