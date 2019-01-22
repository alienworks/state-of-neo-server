using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class ConsensusNode : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Organization { get; set; }

        public string Logo { get; set; }

        public string PublicKey { get; set; }

        public string Address { get; set; }

        public string PublicKeyHash { get; set; }

        public decimal CollectedFees { get; set; }
    }
}
