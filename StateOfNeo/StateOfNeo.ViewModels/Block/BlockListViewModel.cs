using Neo;
using Neo.Wallets;
using StateOfNeo.Common.Extensions;
using System;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockListViewModel : BaseBlockViewModel
    {
        public int TransactionsCount { get; set; }

        public double TimeInSeconds { get; set; }

        public string Validator { get; set; }

        public string ValidatorAddress => UInt160.Parse(this.Validator).ToAddress();

        public decimal CollectedFees { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();
    }
}
