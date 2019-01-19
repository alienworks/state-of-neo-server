using Neo;
using Neo.Wallets;
using StateOfNeo.Common.Extensions;
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
        public double TimeInSeconds { get; set; }
        public string Validator { get; set; }
        public string ValidatorAddress => UInt160.Parse(this.Validator).ToAddress();
        public decimal CollectedFees { get; set; }
        public DateTime CreatedOn => this.Timestamp.ToUnixDate();
    }
}
