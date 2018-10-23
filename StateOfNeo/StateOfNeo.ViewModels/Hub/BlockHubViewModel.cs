using System;

namespace StateOfNeo.ViewModels
{
    public class BlockHubViewModel
    {
        public string Hash { get; set; }
        public int Height { get; set; }
        public int TransactionCount { get; set; }
        public int Size { get; set; }
        public int Timestamp { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
