using Neo.Network.P2P.Payloads;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionDetailedListViewModel : TransactionAssetsViewModel
    {
        public string Hash { get; set; }

        public int Size { get; set; }

        public TransactionType Type { get; set; }

        public long Timestamp { get; set; }

        public string ContractName { get; set; }

        public string ContractHash { get; set; }
    }
}
