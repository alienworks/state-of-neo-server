using System;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockListViewModel
    {
        public string Hash { get; set; }

        public int Height { get; set; }

        public int TransactionsCount { get; set; }

        public int Size { get; set; }

        public long Timestamp { get; set; }

        public DateTime FinalizedAt => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(this.Timestamp).ToLocalTime();
    }
}
