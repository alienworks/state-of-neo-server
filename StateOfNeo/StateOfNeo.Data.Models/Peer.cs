namespace StateOfNeo.Data.Models
{
    public class Peer
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public string Locale { get; set; }
        public string Location { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string FlagUrl { get; set; }

        public int? NodeId { get; set; }
        public virtual Node Node { get; set; }
    }
}
