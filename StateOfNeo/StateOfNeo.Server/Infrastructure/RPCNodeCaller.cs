using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StateOfNeo.Common;
using StateOfNeo.Common.RPC;
using StateOfNeo.Data.Models;
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

namespace StateOfNeo.Server.Infrastructure
{
    public class RPCNodeCaller
    {
        private readonly NetSettings _netSettings;

        public RPCNodeCaller(IOptions<NetSettings> netSettings)
        {
            _netSettings = netSettings.Value;
        }



        public async Task<int?> GetNodeHeight(Node node)
        {
            int? result = null;
            var httpResult = await MakeRPCCall<RPCResponseBody<int>>(node);
            if (httpResult?.Result > 0)
            {
                result = httpResult.Result;
            }
            return result;
        }

        public async Task<int> GetNodeHeight(string ip, uint? port)
        {
            var result = -1;

            if (port == null)
            {
                foreach (var defaultPort in _netSettings.GetPorts())
                {
                    bool isSucessful = false;
                    foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                    {
                        var httpResult = await MakeRPCCall<RPCResponseBody<int>>($"{protocol}://{ip}:{defaultPort.ToString()}");
                        //var httpResult = await MakeRPCCall<RPCResponseBody<int>>(node);
                        if (httpResult.Result > 0)
                        {
                            result = httpResult.Result;
                            isSucessful = true;
                            break;
                        }
                    }
                    if (isSucessful)
                    {
                        break;
                    }
                }
            }
            else
            {
                var httpResult = await MakeRPCCall<RPCResponseBody<int>>($"{ip}:{port}");
                if (httpResult.Result > 0)
                {
                    result = httpResult.Result;
                }
            }
            return result;
        }

        public async Task<string> GetNodeVersion(string endpoint)
        {
            var result = await MakeRPCCall<RPCResponseBody<RPCResultGetVersion>>(endpoint, "getversion");
            if (result != null)
            {
                return result.Result.Useragent;
            }
            return string.Empty;
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
            var result = await MakeRPCCall<RPCResponseBody<RPCPeersResponse>>(node, "getpeers");
            if (result?.Result != null)
            {
                return result.Result;
            }
            return default(RPCPeersResponse);
        }

        private async Task<T> MakeRPCCall<T>(Node node, string method = "getblockcount")
        {
            HttpResponseMessage response = null;
            bool succesfulCall = false;
            var successUrl = string.Empty;
            var url = string.Empty;

            var rpcRequest = new RPCRequestBody
            {
                Method = method
            };

            if (!string.IsNullOrEmpty(node.SuccessUrl))
            {
                response = await SendRPCCall(HttpMethod.Post, $"{node.SuccessUrl}", rpcRequest);
                if (response.IsSuccessStatusCode)
                {
                    successUrl = node.SuccessUrl;
                    succesfulCall = true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(successUrl))
                {
                    if (!string.IsNullOrEmpty(node.Protocol) &&
                     !string.IsNullOrEmpty(node.Url))
                    {
                        foreach (var address in node.NodeAddresses)
                        {
                            if (address.Port.HasValue)
                            {
                                url = $"{node.Protocol}://{node.Url}:{address.Port}";
                                response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                if (response.IsSuccessStatusCode)
                                {
                                    node.Type = NodeAddressType.RPC;
                                    address.Type = NodeAddressType.RPC;
                                    successUrl = url;
                                    break;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(successUrl))
                        {
                            foreach (var port in _netSettings.GetPorts())
                            {
                                url = $"{node.Protocol}://{node.Url}:{port}";
                                response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                if (response.IsSuccessStatusCode)
                                {
                                    node.Type = NodeAddressType.RPC;
                                    successUrl = url;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(successUrl))
                {
                    if (string.IsNullOrEmpty(node.Protocol) &&
                    !string.IsNullOrEmpty(node.Url))
                    {
                        if (node.Url.Contains(RPCCallConstants.PROTOCOL_TYPES_TESTS[0]) ||
                            node.Url.Contains(RPCCallConstants.PROTOCOL_TYPES_TESTS[1]))
                        {
                            foreach (var address in node.NodeAddresses)
                            {
                                if (address.Port.HasValue)
                                {
                                    url = $"{node.Url}:{address.Port}";
                                    response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        node.Type = NodeAddressType.RPC;
                                        address.Type = NodeAddressType.RPC;
                                        successUrl = url;
                                        succesfulCall = true;
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(successUrl))
                            {
                                foreach (var port in _netSettings.GetPorts())
                                {
                                    url = $"{node.Url}:{port}";
                                    response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        node.Type = NodeAddressType.RPC;
                                        successUrl = url;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var address in node.NodeAddresses)
                            {
                                if (address.Port.HasValue)
                                {
                                    foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                                    {
                                        url = $"{protocol}://{node.Url}:{address.Port}";
                                        response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            node.Type = NodeAddressType.RPC;
                                            address.Type = NodeAddressType.RPC;
                                            successUrl = url;
                                            succesfulCall = true;
                                            break;
                                        }
                                    }
                                }
                                if (succesfulCall) break;
                            }
                            if (string.IsNullOrEmpty(successUrl))
                            {
                                foreach (var port in _netSettings.GetPorts())
                                {
                                    foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                                    {
                                        url = $"{protocol}://{node.Url}:{port}";
                                        response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            node.Type = NodeAddressType.RPC;
                                            successUrl = url;
                                            succesfulCall = true;
                                            break;
                                        }
                                    }
                                    if (succesfulCall)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(successUrl))
                {
                    foreach (var address in node.NodeAddresses)
                    {
                        if (address.Port.HasValue)
                        {
                            foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                            {
                                url = $"{protocol}://{address.Ip}:{address.Port}";
                                response = await SendRPCCall(HttpMethod.Post, $"{protocol}://{address.Ip}:{address.Port}", rpcRequest);
                                if (response.IsSuccessStatusCode)
                                {
                                    node.Type = NodeAddressType.RPC;
                                    address.Type = NodeAddressType.RPC;
                                    successUrl = url;
                                    succesfulCall = true;
                                    break;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(successUrl))
                        {
                            foreach (var port in _netSettings.GetPorts())
                            {
                                foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                                {
                                    url = $"{protocol}://{address.Ip}:{port}";
                                    response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        node.Type = NodeAddressType.RPC;
                                        address.Type = NodeAddressType.RPC;
                                        successUrl = url;
                                        succesfulCall = true;
                                        break;
                                    }
                                }
                                if (succesfulCall)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(successUrl))
            {
                node.SuccessUrl = successUrl;
                if (response != null && response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var serializedResult = JsonConvert.DeserializeObject<T>(result);
                    return serializedResult;
                }
            }

            return default(T);
        }

        private async Task<T> MakeRPCCall<T>(string endpoint, string method = "getblockcount")
        {
            var rpcRequest = new RPCRequestBody
            {
                Method = method
            };
            var response = await SendRPCCall(HttpMethod.Post, endpoint, rpcRequest);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var serializedResult = JsonConvert.DeserializeObject<T>(result);
                return serializedResult;
            }
            return default(T);
        }

        private async Task<HttpResponseMessage> SendRPCCall(HttpMethod httpMethod, string endpoint, object rpcData)
        {
            HttpResponseMessage response;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var http = new HttpClient())
                {
                    var req = new HttpRequestMessage(httpMethod, $"{endpoint}");
                    var data = JsonConvert.SerializeObject(rpcData, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    //http.Timeout = TimeSpan.FromSeconds(3);
                    req.Content = new StringContent(data, Encoding.Default, "application/json");
                    response = await http.SendAsync(req);
                }
            }
            catch (Exception e)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            stopwatch.Stop();
            return response;
        }
    }
}
