using Neo.Network.P2P.Payloads;
using StateOfNeo.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionDetailsViewModel : TransactionAssetsViewModel
    {
        public string Hash { get; set; }

        public int Size { get; set; }

        public TransactionType Type { get; set; }

        public long Timestamp { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();

        public decimal NetworkFee { get; set; }

        public decimal SystemFee { get; set; }

        public int Version { get; set; }

        public string BlockHash { get; set; }

        public int BlockHeight { get; set; }

        public string ContractName { get; set; }

        public string ContractHash { get; set; }

        public IEnumerable<TransactionAttributeViewModel> Attributes { get; set; }

        public IEnumerable<TransactionWitnessViewModel> Witnesses { get; set; }
    }
}
