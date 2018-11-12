using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionAttributeViewModel
    {
        public int Usage { get; set; }

        public string Type => ((TransactionAttributeUsage)this.Usage).ToString();

        public string DataAsHexString { get; set; }
    }
}
