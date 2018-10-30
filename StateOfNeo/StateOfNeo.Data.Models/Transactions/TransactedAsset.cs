using StateOfNeo.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class TransactedAsset : BaseEntity
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }
        
        public GlobalAssetType AssetType { get; set; }
        
        public int FromAddressId { get; set; }

        public virtual Address FromAddress { get; set; }

        public int ToAddressId { get; set; }

        public virtual Address ToAddress { get; set; }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }

        public int AssetId { get; set; }

        public virtual Asset Asset { get; set; }
    }
}
