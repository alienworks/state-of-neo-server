namespace StateOfNeo.Common
{
    public class NetSettings
    {
        public string Net { get; set; }
        public int[] MainNetPorts { get; set; }
        public int[] TestNetPorts { get; set; }

        public int[] GetPorts()
        {
            if (this.Net == NetConstants.MAIN_NET)
            {
                return MainNetPorts;
            }

            return TestNetPorts;
        }
    }
}
