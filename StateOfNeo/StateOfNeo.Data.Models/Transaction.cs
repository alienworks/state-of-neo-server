using StateOfNeo.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class Transaction : BaseEntity
    {
        [Key]
        public string ScriptHash { get; set; }

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public GlobalAssetType AssetType { get; set; }

        public int FromAddressId { get; set; }

        public virtual Address FromAddress { get; set; }

        public int ToAddressId { get; set; }

        public virtual Address ToAddress { get; set; }

        public int BlockId { get; set; }

        public virtual Block Block { get; set; }    
    }
}
