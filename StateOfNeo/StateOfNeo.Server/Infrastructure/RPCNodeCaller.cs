using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StateOfNeo.Common;
using StateOfNeo.Common.Extensions;
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
        private uint BlockCount = 0;

        public RPCNodeCaller(IOptions<NetSettings> netSettings)
        {
            this.netSettings = netSettings.Value;
        }

        //public async Task Init()
        //{
        //    await this.Run();
        //}

        //// Specify what you want to happen when the Elapsed event is raised.
        //private void OnTimedEvent(object source, ElapsedEventArgs e)
        //{
        //    this.Run().Wait();
        //}

        public async Task Run(StateOfNeoContext db, long timestamp, uint blockHeight)
        {
            if (this.BlockCount != blockHeight)
            {
                this.BlockCount = blockHeight;
                var nodes = db.Nodes.Skip(2).ToList();

                foreach (var node in nodes)
                {
                    await this.UpdateNodeInfo(db, node);
                }
            }
        }

        public async Task UpdateNodeInfo(StateOfNeoContext db, int nodeId)
        {
            if (db.Nodes.Any(x => x.Id == nodeId))
            {
                await this.UpdateNodeInfo(db, db.Nodes.First(x => x.Id == nodeId));
            }
        }

        public async Task UpdateNodeInfo(StateOfNeoContext db, Node node)
        {
            var stopwatch = Stopwatch.StartNew();
            var height = await this.GetNodeHeight(node);
            stopwatch.Stop();
            var latency = stopwatch.ElapsedMilliseconds;

            if (!height.HasValue)
            {
                node.Height = height;
            }

            db.Nodes.Update(node);
            await db.SaveChangesAsync();
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
            var result = await RpcCaller.MakeRPCCall<RPCResponseBody<RPCResponseVersion>>(endpoint, "getversion");
            return result == null ? string.Empty : result.Result.Useragent;
        }

        public async Task<string> GetNodeVersion(Node node)
        {
            if (string.IsNullOrEmpty(node.Version))
            {
                var result = await MakeRPCCall<RPCResponseBody<RPCResponseVersion>>(node, "getversion");
                if (result?.Result != null)
                {
                    return result.Result.Useragent;
                }
            }

            return node.Version;
        }

        public async Task<RPCResponsePeers> GetNodePeers(Node node)
        {
            var result = await this.MakeRPCCall<RPCResponseBody<RPCResponsePeers>>(node, "getpeers");
            return result?.Result;
        }

        private async Task<T> MakeRPCCall<T>(Node node, string method = "getblockcount")
        {
            try
            {
                HttpResponseMessage response = null;
                var rpcRequest = new RPCRequestBody(method);

                if (!string.IsNullOrEmpty(node.SuccessUrl))
                {
                    response = await RpcCaller.SendRPCCall(HttpMethod.Post, $"{node.SuccessUrl}", rpcRequest);
                }
                else
                {
                    var ports = this.netSettings.GetPorts();
                    foreach (var portWithType in ports)
                    {
                        var triedUrl = default(string);
                        if (!string.IsNullOrEmpty(node.Url))
                        {
                            triedUrl = portWithType.GetFullUrl(node.Url);
                            response = await RpcCaller.SendRPCCall(HttpMethod.Post, triedUrl, rpcRequest);
                        }
                        else
                        {
                            triedUrl = portWithType.GetFullUrl(node.NodeAddresses.FirstOrDefault().Ip);
                            response = await RpcCaller.SendRPCCall(HttpMethod.Post, triedUrl, rpcRequest);
                        }

                        if (response != null && response.IsSuccessStatusCode)
                        {
                            node.SuccessUrl = triedUrl;
                            break;
                        }
                    }
                }

                if (response != null && response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var serializedResult = JsonConvert.DeserializeObject<T>(result);
                    return serializedResult;
                }
            }
            catch (Exception e)
            {

            }

            return default(T);
        }
    }
}
