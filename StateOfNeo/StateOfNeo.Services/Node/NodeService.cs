using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Options;
using StateOfNeo.Common;
using StateOfNeo.Data;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public class NodeService : INodeService
    {
        private readonly StateOfNeoContext ctx;
        private readonly NetSettings netSettings;

        public NodeService(StateOfNeoContext ctx, IOptions<NetSettings> netSettings)
        {
            this.ctx = ctx;
            this.netSettings = netSettings.Value;
        }

        public async Task<IPagedList<T>> GetPage<T>(int page = 1, int pageSize = 10)
        {
            var result = await this.ctx.Nodes
                .Where(x => x.SuccessUrl != null && x.Net == this.netSettings.Net)
                .ProjectTo<T>()
                .ToPagedListAsync(page, pageSize);

            return result;
        }
    }
}
