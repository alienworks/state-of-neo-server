using System;
using System.Collections.Generic;
using StateOfNeo.Data.Models.Transactions;

namespace StateOfNeo.Data.Models
{
    public class AssetInTransaction : BaseEntity
    {
        public AssetInTransaction()
        {
            this.AddressesInAssetTransactions = new HashSet<AddressInAssetTransaction>();
        }

        public int Id { get; set; }
        
        public string TransactionHash { get; set; }

        public virtual Transaction Transaction { get; set; }

        public string AssetHash { get; set; }

        public virtual Asset Asset { get; set; }

        public virtual ICollection<AddressInAssetTransaction> AddressesInAssetTransactions { get; set; }
    }
}
