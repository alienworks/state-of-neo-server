namespace StateOfNeo.Data.Models
{
    public class NodeAudit : StampedEntity
    {
        public long Id { get; set; }

        public int Latency { get; set; }

        public decimal Peers { get; set; }

        public int NodeId { get; set; }

        public virtual Node Node { get; set; }
    }
}
