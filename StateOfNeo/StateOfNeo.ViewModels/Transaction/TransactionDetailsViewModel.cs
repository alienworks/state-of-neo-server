using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionDetailsViewModel
    {
        public string Hash { get; set; }

        public int Size { get; set; }

        public TransactionType Type { get; set; }

        public long Timestamp { get; set; }

        public DateTime FinalizedAt { get; set; }

        public decimal NetworkFee { get; set; }

        public decimal SystemFee { get; set; }

        public int Version { get; set; }

        public string BlockHash { get; set; }

        public int BlockHeight { get; set; }

        public IEnumerable<TransactedAssetViewModel> GlobalIncomingAssets { get; set; }

        public IEnumerable<TransactedAssetViewModel> GlobalOutgoingAssets { get; set; }

        public IEnumerable<TransactedAssetViewModel> Assets { get; set; }

        public IEnumerable<TransactionAttributeViewModel> Attributes { get; set; }

        public IEnumerable<TransactionWitnessViewModel> Witnesses { get; set; }
    }
}
