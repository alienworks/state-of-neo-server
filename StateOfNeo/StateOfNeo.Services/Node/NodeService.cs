using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Options;
using StateOfNeo.Common;
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
using System.Threading;
using System;
using Serilog;

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
                .Include(x => x.NodeAddresses)
                .Where(x => x.Net.ToLower() == this.netSettings.Net.ToLower())
                .Where(x => x.SuccessUrl != null)
                .ProjectTo<T>()
                .ToList();

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

        public IEnumerable<ChartStatsViewModel> NodeTypesChart()
        {
            var result = this.db.Nodes
                .Where(x => x.Type == NodeAddressType.REST || x.Type == NodeAddressType.RPC)
                .GroupBy(x => x.Type)
                .Select(x => new ChartStatsViewModel
                {
                    Label = x.Key.ToString(),
                    Value = x.Count()
                })
                .ToList();

            result.Add(new ChartStatsViewModel { Label = "Consensus", Value = 7 });

            return result;
        }

        public async Task<bool> GetWsStatusAsync(int nodeId)
        {
            var nodeUrl = this.db.Nodes
                .Where(x => x.Id == nodeId)
                .Select(x => $"ws://{x.Url}:10334")
                .FirstOrDefault();

            var websocket = new System.Net.WebSockets.ClientWebSocket();
            var success = false;

            try
            {
                await websocket.ConnectAsync(new System.Uri(nodeUrl), CancellationToken.None);
                success = websocket.State == System.Net.WebSockets.WebSocketState.Open;
                await websocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            catch (Exception e)
            {
                Log.Error("WS connection CloseAsync() ", e);
            }
            finally
            {
                websocket.Dispose();
            }

            return success;
        }
    }
}
