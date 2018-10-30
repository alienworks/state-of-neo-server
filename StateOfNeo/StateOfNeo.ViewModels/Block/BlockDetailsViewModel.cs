using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Block
{
    public class BlockDetailsViewModel
    {
        public string Hash { get; set; }

        public int Height { get; set; }

        public long Timestamp { get; set; }

        public int Size { get; set; }

        public ulong ConsensusData { get; set; }

        public string NextConsensusNodeAddress { get; set; }

        public string Validator { get; set; }

        public string InvocationScript { get; set; }

        public string VerificationScript { get; set; }
    }
}
