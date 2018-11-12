using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StateOfNeo.Common;
using StateOfNeo.Common.RPC;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Infrastructure.RPC;
using StateOfNeo.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StateOfNeo.Server.Infrastructure
{
    public class RPCNodeCaller
    {
        private readonly NetSettings netSettings;
        private readonly IHubContext<NodeHub> nodeHub;
        private readonly StateOfNeoContext db;
        private int BlockCount = 0;

        public RPCNodeCaller(
            StateOfNeoContext db,
            IHubContext<NodeHub> nodeHub,
            IOptions<NetSettings> netSettings)
        {
            this.db = db;
            this.nodeHub = nodeHub;
            this.netSettings = netSettings.Value;
        }

        public async Task Init()
        {
            await this.Run();
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Interval = 5 * 60 * 1000;
            aTimer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.Run().Wait();
        }

        private async Task Run()
        {
            var latestBlockCount = this.db.Blocks.Count();
            if (this.BlockCount != latestBlockCount)
            {
                this.BlockCount = latestBlockCount;
                var nodes = this.db.Nodes.Where(x => x.Net == this.netSettings.Net).Skip(2).ToList();

                foreach (var node in nodes)
                {
                    await this.UpdateNodeInfo(node);
                }
            }
        }

        public async Task UpdateNodeInfo(int nodeId)
        {
            if (this.db.Nodes.Any(x => x.Id == nodeId))
            {
                await this.UpdateNodeInfo(this.db.Nodes.First(x => x.Id == nodeId));
            }
        }

        public async Task UpdateNodeInfo(Node node)
        {
            var stopwatch = Stopwatch.StartNew();
            var height = await this.GetNodeHeight(node);
            stopwatch.Stop();
            var latency = stopwatch.ElapsedMilliseconds;

            if (height != null)
            {
                node.Height = height;
            }

            this.db.Nodes.Update(node);
            await this.db.SaveChangesAsync();

            await this.nodeHub.Clients.All.SendAsync("NodeInfo", node.SuccessUrl);
        }

        public async Task<int?> GetNodeHeight(Node node)
        {
            int? result = null;
            var httpResult = await this.MakeRPCCall<RPCResponseBody<int>>(node);
            if (httpResult?.Result > 0)
            {
                result = httpResult.Result;
            }

            return result;
        }

        public async Task<string> GetNodeVersion(string endpoint)
        {
            var result = await RpcCaller.MakeRPCCall<RPCResponseBody<RPCResultGetVersion>>(endpoint, "getversion");
            return result == null ? string.Empty : result.Result.Useragent;
        }

        public async Task<string> GetNodeVersion(Node node)
        {
            if (string.IsNullOrEmpty(node.Version))
            {
                var result = await MakeRPCCall<RPCResponseBody<RPCResultGetVersion>>(node, "getversion");
                if (result?.Result != null)
                {
                    return result.Result.Useragent;
                }
            }

            return node.Version;
        }

        public async Task<RPCPeersResponse> GetNodePeers(Node node)
        {
            var result = await this.MakeRPCCall<RPCResponseBody<RPCPeersResponse>>(node, "getpeers");
            return result?.Result;
        }

        private async Task<T> MakeRPCCall<T>(Node node, string method = "getblockcount")
        {
            HttpResponseMessage response = null;
            var rpcRequest = new RPCRequestBody(method);

            if (!string.IsNullOrEmpty(node.SuccessUrl))
            {
                response = await RpcCaller.SendRPCCall(HttpMethod.Post, $"{node.SuccessUrl}", rpcRequest);
            }
            else
            {
                foreach (var portWithType in this.netSettings.GetPorts())
                {
                    if (!string.IsNullOrEmpty(node.Url))
                    {
                        response = await RpcCaller.SendRPCCall(HttpMethod.Post, portWithType.GetFullUrl(node.Url), rpcRequest);
                    }
                    else
                    {
                        response = await RpcCaller.SendRPCCall(HttpMethod.Post, portWithType.GetFullUrl(node.NodeAddresses.FirstOrDefault().Ip), rpcRequest);
                    }

                    if (response != null && response.IsSuccessStatusCode)
                        break;
                }
            }

            if (response != null && response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var serializedResult = JsonConvert.DeserializeObject<T>(result);
                return serializedResult;
            }

            return default(T);
        }
    }
}
