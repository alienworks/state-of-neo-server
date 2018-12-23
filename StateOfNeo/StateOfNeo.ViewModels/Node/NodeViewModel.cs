namespace StateOfNeo.ViewModels
{
    public class NodeViewModel : NodeListViewModel
    {
        public uint? Port { get; set; }
        public string Protocol { get; set; }

        public string Version { get; set; }
        public string Type { get; set; }
        public string Service { get; set; }

        public int? Height { get; set; }
        public int? Peers { get; set; }
        public int? MemoryPool { get; set; }
        public string FlagUrl { get; set; }

        public string Locale { get; set; }
        public string Location { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public bool IsVisited { get; set; }

        public string Net { get; set; }

        public override int GetHashCode() => 
            (this.Ip + this.Protocol + this.Port).GetHashCode();
    }
}
