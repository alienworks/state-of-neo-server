namespace StateOfNeo.Data.Models
{
    public abstract class StampedEntity : BaseEntity
    {
        public long Timestamp { get; set; }
    }
}
