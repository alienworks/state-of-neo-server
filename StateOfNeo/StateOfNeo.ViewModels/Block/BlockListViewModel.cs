using StateOfNeo.Common.Extensions;
using System;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockListViewModel : BaseBlockViewModel
    {
        public int TransactionsCount { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();
    }
}
