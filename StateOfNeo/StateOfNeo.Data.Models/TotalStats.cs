namespace StateOfNeo.Data.Models
{
    public class TotalStats
    {
        public int Id { get; set; }

        public int? BlockCount { get; set; }
        public decimal? BlocksTimes { get; set; }
        public long? BlocksSizes { get; set; }

        public decimal? ClaimedGas { get; set; }
        public long? TransactionsCount { get; set; }

        public int? AddressCount { get; set; }
        public int? AssetsCount { get; set; }

        public long? NeoGasTxCount { get; set; }
        public long? Nep5TxCount { get; set; }

        public long? Timestamp { get; set; }
    }
}
