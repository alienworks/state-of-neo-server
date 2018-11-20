using System;

namespace StateOfNeo.ViewModels
{
    public class HeaderStatsViewModel
    {
        public string Hash { get; set; }
        public int Height { get; set; }
        public int TransactionCount { get; set; }
        public int Size { get; set; }
        public long Timestamp { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
