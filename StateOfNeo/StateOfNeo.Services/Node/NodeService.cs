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

using WebSocket4Net;
using Microsoft.EntityFrameworkCore;

namespace StateOfNeo.Services
{
    public class NodeService : FilterService, INodeService
    {
        private readonly NetSettings netSettings;

        public NodeService(StateOfNeoContext db, IOptions<NetSettings> netSettings) : base(db)
        {
            this.netSettings = netSettings.Value;
        }

        public T Get<T>(int id) =>
            this.db.Nodes
                .Where(x => x.Id == id)
                .ProjectTo<T>()
                .FirstOrDefault();

        public IEnumerable<T> GetNodes<T>() =>
            this.db.Nodes
                .Include(n => n.NodeAddresses)
                .Where(n => n.Net.ToLower() == this.netSettings.Net.ToLower())
                .Where(x => x.SuccessUrl != null)
                .ProjectTo<T>();

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
                x => new ValueExtractionModel { Size = x.Peers.Value, Timestamp = x.Timestamp },
                x => x.NodeId == nodeId && x.Peers.HasValue);
        }

        public async Task<bool> GetWsStatusAsync(int nodeId)
        {
            var nodeUrl = this.db.Nodes
                .Where(x => x.Id == nodeId)
                .Select(x => $"ws://{x.Url}:10334")
                .FirstOrDefault();

            var websocket = new WebSocket(nodeUrl);
            await websocket.OpenAsync();

            while (websocket.State == WebSocketState.Connecting) { }
            var success = websocket.State == WebSocketState.Open;

            await websocket.CloseAsync();
            return success;
        }
    }
}
