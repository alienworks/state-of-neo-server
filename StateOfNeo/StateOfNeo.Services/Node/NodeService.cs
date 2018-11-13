using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Options;
using StateOfNeo.Common;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Common.Helpers.Models;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Chart;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public class NodeService : FilterService, INodeService
    {
        private readonly NetSettings netSettings;

        public NodeService(StateOfNeoContext db, IOptions<NetSettings> netSettings) : base(db)
        {
            this.netSettings = netSettings.Value;
        }

        public async Task<IPagedList<T>> GetPage<T>(int page = 1, int pageSize = 10)
        {
            var result = await this.db.Nodes
                .Where(x => x.SuccessUrl != null && x.Net == this.netSettings.Net)
                .ProjectTo<T>()
                .ToPagedListAsync(page, pageSize);

            return result;
        }

        public IEnumerable<ChartStatsViewModel> LatencyChart(ChartFilterViewModel filter, int nodeId)
        {
            return this.Filter<NodeAudit>(filter,
                x => new ValueExtractionModel { Size = x.Latency, Timestamp = x.Timestamp },
                x => x.NodeId == nodeId);
        }

        public IEnumerable<ChartStatsViewModel> PeersChart(ChartFilterViewModel filter, int nodeId)
        {
            return this.Filter<NodeAudit>(filter,
                x => new ValueExtractionModel { Size = x.Peers, Timestamp = x.Timestamp },
                x => x.NodeId == nodeId);
        }
    }
}
