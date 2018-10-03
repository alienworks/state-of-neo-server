using Newtonsoft.Json;

namespace StateOfNeo.Common.RPC
{
    public class RPCBaseBody
    {
        [JsonProperty(PropertyName = "jsonrpc")]
        public string Jsonrpc { get; set; } = "2.0";

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; } = 1;
    }
}
