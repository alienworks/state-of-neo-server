using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class HttpRequester
    {


        public static async Task<T> MakeRestCall<T>(string url, HttpMethod method)
        {
            try
            {
                HttpResponseMessage response;

                HttpClient httpClient = new HttpClient();
                var req = new HttpRequestMessage(method, url);
                response = await httpClient.SendAsync(req);

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
