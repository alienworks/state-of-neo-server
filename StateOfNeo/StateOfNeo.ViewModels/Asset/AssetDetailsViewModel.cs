
namespace StateOfNeo.ViewModels.Asset
{
    public class AssetDetailsViewModel
    {
        public string Name { get; set; }

        public string Hash { get; set; }

        public decimal TotalSupply { get; set; }

        public string Address { get; set; }

        public int TransactionsCount { get; set; }

        public int AddressesCount { get; set; }
    }
}
