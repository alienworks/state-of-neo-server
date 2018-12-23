using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class AddressAssetBalance : BaseEntity
    {
        public int Id { get; set; }

        public decimal Balance { get; set; }

        public int TransactionsCount { get; set; }

        public string AddressPublicAddress { get; set; }

        public virtual Address Address { get; set; }

        public string AssetHash { get; set; }

        public virtual Asset Asset { get; set; }
    }
}
