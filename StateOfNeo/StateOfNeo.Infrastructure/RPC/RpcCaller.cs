using Newtonsoft.Json;
using StateOfNeo.Common.RPC;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Infrastructure.RPC
{
    public class RpcCaller
    {
        public static async Task<T> MakeRPCCall<T>(string endpoint, string method = "getblockcount")
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

        public static async Task<HttpResponseMessage> SendRPCCall(HttpMethod httpMethod, string endpoint, object rpcData)
        {
            HttpResponseMessage response;
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

            return response;
        }
    }
}
