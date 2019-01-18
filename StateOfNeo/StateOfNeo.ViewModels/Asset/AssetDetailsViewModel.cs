
namespace StateOfNeo.ViewModels.Asset
{
    public class AssetDetailsViewModel
    {
        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Hash { get; set; }

        public int Decimals { get; set; }

        public long TotalSupply { get; set; }

        public int TransactionsCount { get; set; }
        public int TransactionsCount1 { get; set; }
        public int TransactionsCount2 { get; set; }
        public int TransactionsCount3 { get; set; }

        public int NewAddressesLastMonth { get; set; }

        public int ActiveAddressesLastMonth { get; set; }

        public double AverageTransactedValue { get; set; }

        public double MedianTransactedValue { get; set; }
    }
}
