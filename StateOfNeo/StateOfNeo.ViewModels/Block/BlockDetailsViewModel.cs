using StateOfNeo.Common.Extensions;
using System;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockDetailsViewModel : BaseBlockViewModel
    {
        public ulong ConsensusData { get; set; }

        public string NextConsensusNodeAddress { get; set; }

        public string Validator { get; set; }

        public string InvocationScript { get; set; }

        public string VerificationScript { get; set; }

        public string PreviousBlockHash { get; set; }

        public double SecondsFromPreviousBlock { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();
    }
}
