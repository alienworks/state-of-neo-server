using Newtonsoft.Json;

namespace StateOfNeo.Common.RPC
{
    public class RPCPeer
    {
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        [JsonProperty(PropertyName = "port")]
        public uint Port { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is RPCPeer))
            {
                return false;
            }

            var otherPeer = obj as RPCPeer;
            return this.GetHashCode() == otherPeer.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ($"{this.Address}:{this.Port}").GetHashCode();
        }
    }
}
