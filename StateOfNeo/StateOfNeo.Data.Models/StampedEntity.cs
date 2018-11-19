namespace StateOfNeo.Data.Models
{
    public abstract class StampedEntity : BaseEntity
    {
        public long Timestamp { get; set; }

        public long MonthlyStamp { get; set; }

        public long DailyStamp { get; set; }

        public long HourlyStamp { get; set; }
    }
}
