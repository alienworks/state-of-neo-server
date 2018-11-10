using System.Collections.Generic;

namespace StateOfNeo.Common
{
    public class NetSettings
    {
        public string Net { get; set; }
        public List<PortWithType> MainNetPorts { get; set; }
        public List<PortWithType> TestNetPorts { get; set; }
        public List<PortWithType> CommonPorts { get; set; }

        public List<PortWithType> GetPorts()
        {
            var result = this.TestNetPorts;
            if (this.Net == NetConstants.MAIN_NET)
            {
                result = this.MainNetPorts;
            }

            result.AddRange(this.CommonPorts);
            return result;
        }
    }

    public class PortWithType
    {
        public string Type { get; set; }
        public int Port { get; set; }

        public string GetFullUrl(string urlOrIp)
        {
            return $@"{this.Type}:/{urlOrIp}:{this.Port}";
        }
    }
}
