using Newtonsoft.Json;

namespace StateOfNeo.Common.RPC
{
    public class RPCResponseVersion
    {
        [JsonProperty(PropertyName = "port")]
        public uint Port { get; set; }

        [JsonProperty(PropertyName = "nonce")]
        public uint Nonce { get; set; }

        [JsonProperty(PropertyName = "useragent")]
        public string Useragent { get; set; }
    }
}
