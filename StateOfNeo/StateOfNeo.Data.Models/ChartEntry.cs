using StateOfNeo.Common.Enums;

namespace StateOfNeo.Data.Models
{
    public class ChartEntry
    {
        public int Id { get; set; }
        public ChartEntryType Type { get; set; }
        public UnitOfTime UnitOfTime { get; set; }
        public long Timestamp { get; set; }
        public long? Count { get; set; }
        public decimal Value { get; set; }
        public decimal? AccumulatedValue { get; set; }
    }
}
