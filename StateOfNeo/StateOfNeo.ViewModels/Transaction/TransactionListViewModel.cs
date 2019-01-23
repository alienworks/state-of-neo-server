using Neo.Network.P2P.Payloads;
using StateOfNeo.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionListViewModel : TransactionAssetsViewModel
    {
        public string Hash { get; set; }

        public int Size { get; set; }

        public TransactionType Type { get; set; }

        public long Timestamp { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();
        
    }
}
