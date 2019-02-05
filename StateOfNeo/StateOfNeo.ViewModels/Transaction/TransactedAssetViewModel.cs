using StateOfNeo.Common.Enums;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactedAssetViewModel
    {
        public decimal Amount { get; set; }

        public AssetType AssetType { get; set; }

        public string AssetHash { get; set; }

        public string AssetSymbol { get; set; }

        public string FromAddress { get; set; }

        public string ToAddress { get; set; }

        public string Name { get; set; }
    }
}
