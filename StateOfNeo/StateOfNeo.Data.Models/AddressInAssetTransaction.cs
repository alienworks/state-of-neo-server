using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class AddressInAssetTransaction : BaseEntity
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public int AssetInTransactionId { get; set; }

        public virtual AssetInTransaction AssetInTransaction { get; set; }

        public string AddressPublicAddress { get; set; }

        public virtual Address Address { get; set; }
    }
}
