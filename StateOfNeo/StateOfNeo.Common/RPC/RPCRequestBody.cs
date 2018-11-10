using Newtonsoft.Json;

namespace StateOfNeo.Common.RPC
{
    public class RPCRequestBody : RPCBaseBody
    {
        public RPCRequestBody() { }
        public RPCRequestBody(string method)
        {
            this.Method = method;
        }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; } = "getblockcount";
        [JsonProperty(PropertyName = "params")]
        public string[] Params { get; set; } = new string[] { };
    }
}
