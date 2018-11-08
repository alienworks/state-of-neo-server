namespace StateOfNeo.Data.Models
{
    public abstract class StampedEntity : BaseEntity
    {
        public uint Timestamp { get; set; }
    }
}
