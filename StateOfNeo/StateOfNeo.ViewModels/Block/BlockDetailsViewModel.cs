using Neo;
using Neo.Wallets;
using StateOfNeo.Common.Extensions;
using System;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockDetailsViewModel : BaseBlockViewModel
    {
        public ulong ConsensusData { get; set; }

        public string NextConsensusNodeAddress { get; set; }

        public string Validator { get; set; }

        public string ValidatorAddress => UInt160.Parse(this.Validator).ToAddress();

        public string InvocationScript { get; set; }

        public string VerificationScript { get; set; }

        public string PreviousBlockHash { get; set; }

        public string NextBlockHash { get; set; }

        public double SecondsFromPreviousBlock { get; set; }

        public decimal CollectedFees { get; set; }

        public int TransactionsCount { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();
    }
}
