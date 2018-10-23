namespace StateOfNeo.ViewModels
{
    public class NodeViewModel
    {
        public int Id { get; set; }
        public uint? Port { get; set; }
        public string Ip { get; set; }
        public string Protocol { get; set; }
        public string Url { get; set; }

        public string Version { get; set; }
        public string Type { get; set; }

        public int? Height { get; set; }
        public int? Peers { get; set; }
        public int? MemoryPool { get; set; }
        public string FlagUrl { get; set; }
        public string SuccessUrl { get; set; }

        public string Locale { get; set; }
        public string Location { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public bool IsVisited { get; set; }

        public string Net { get; set; }

        public override int GetHashCode()
        {
            var hash = (Ip + Protocol + Port).GetHashCode();
            return hash;
        }
    }
}
