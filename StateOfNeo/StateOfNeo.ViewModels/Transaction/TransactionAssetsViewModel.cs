using System.Collections.Generic;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionAssetsViewModel
    {
        public IEnumerable<TransactedAssetViewModel> GlobalIncomingAssets { get; set; }

        public IEnumerable<TransactedAssetViewModel> GlobalOutgoingAssets { get; set; }

        public IEnumerable<TransactedAssetViewModel> Assets { get; set; }
    }
}
