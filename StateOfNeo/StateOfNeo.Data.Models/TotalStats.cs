namespace StateOfNeo.Data.Models
{
    public class TotalStats
    {
        public int Id { get; set; }

        public int BlockCount { get; set; }
        public double BlocksTimes;
        public long BlocksSizes;

        public decimal ClaimedGas { get; set; }
        public int TransactionsCount { get; set; }

        public int AddressCount;
        public int AssetsCount;

        public long Timestamp { get; set; }
    }
}
