using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using StateOfNeo.Data.Models.Transactions;

namespace StateOfNeo.Data.Models
{
    public class Block : StampedEntity
    {
        public Block()
        {
            this.Transactions = new HashSet<Transaction>();
            this.NodeStatusUpdates = new HashSet<NodeStatus>();
        }

        [Key]
        public string Hash { get; set; }

        public int Height { get; set; }

        public int Size { get; set; }

        public double TimeInSeconds { get; set; }

        public ulong ConsensusData { get; set; }

        public string NextConsensusNodeAddress { get; set; }

        public string Validator { get; set; }

        public string InvocationScript { get; set; }

        public string VerificationScript { get; set; }

        public string PreviousBlockHash { get; set; }

        public virtual Block PreviousBlock { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual ICollection<NodeStatus> NodeStatusUpdates { get; set; }
    }
}
