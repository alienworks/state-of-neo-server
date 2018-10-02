using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class Block : BaseEntity
    {
        public Block()
        {
            this.Transactions = new HashSet<Transaction>();
            this.NodeStatusUpdates = new HashSet<NodeStatusUpdate>();
        }

        [Key]
        public string Hash { get; set; }

        public int Height { get; set; }

        public long Timestamp { get; set; }

        public int Size { get; set; }

        public string Validator { get; set; }

        public string InvocationScript { get; set; }

        public string VerificationScript { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual ICollection<NodeStatusUpdate> NodeStatusUpdates { get; set; }
    }
}
