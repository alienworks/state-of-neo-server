namespace StateOfNeo.ViewModels
{
    public class PeerViewModel
    {
        public int Id { get; set; }
        public int NodeId { get; set; }
        public string Ip { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string FlagUrl { get; set; }
    }
}
