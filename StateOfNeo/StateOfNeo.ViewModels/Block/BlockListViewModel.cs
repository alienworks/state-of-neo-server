using StateOfNeo.Common.Extensions;
using System;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockListViewModel : BaseBlockViewModel
    {
        public int TransactionsCount { get; set; }

        public double TimeInSeconds { get; set; }

        public string Validator { get; set; }

        public decimal CollectedFees { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();
    }
}
