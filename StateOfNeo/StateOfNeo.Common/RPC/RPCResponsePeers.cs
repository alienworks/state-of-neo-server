using Newtonsoft.Json;
using System.Collections.Generic;

namespace StateOfNeo.Common.RPC
{
    public class RPCResponsePeers
    {
        [JsonProperty(PropertyName = "unconnected")]
        public IEnumerable<RPCPeer> Unconnected { get; set; }
        [JsonProperty(PropertyName = "bad")]
        public IEnumerable<RPCPeer> Bad { get; set; }
        [JsonProperty(PropertyName = "connected")]
        public IEnumerable<RPCPeer> Connected { get; set; }
    }
}
