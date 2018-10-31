using System;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockListViewModel : BaseBlockViewModel
    {
        public int TransactionsCount { get; set; }

        public DateTime FinalizedAt => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(this.Timestamp).ToLocalTime();
    }
}
