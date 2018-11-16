using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StateOfNeo.Data.Models.Transactions
{
    public class Transaction : StampedEntity
    {
        public Transaction()
        {
            this.AddressesInTransactions = new HashSet<AddressInTransaction>();
            this.AssetsInTransactions = new HashSet<AssetInTransaction>();
            this.Assets = new HashSet<TransactedAsset>();
            this.Attributes = new HashSet<TransactionAttribute>();
            this.GlobalIncomingAssets = new HashSet<TransactedAsset>();
            this.GlobalOutgoingAssets = new HashSet<TransactedAsset>();
            this.Witnesses = new HashSet<TransactionWitness>();
        }

        [Key]
        public string Hash { get; set; }

        public Neo.Network.P2P.Payloads.TransactionType Type { get; set; }

        public decimal NetworkFee { get; set; }

        public decimal SystemFee { get; set; }

        public int Size { get; set; }

        public int Version { get; set; }

        public string BlockId { get; set; }

        public virtual Block Block { get; set; }
        
        [InverseProperty(nameof(TransactedAsset.InGlobalTransaction))]
        public virtual ICollection<TransactedAsset> GlobalIncomingAssets { get; set; }

        [InverseProperty(nameof(TransactedAsset.OutGlobalTransaction))]
        public virtual ICollection<TransactedAsset> GlobalOutgoingAssets { get; set; }

        [InverseProperty(nameof(TransactedAsset.Transaction))]
        public virtual ICollection<TransactedAsset> Assets { get; set; }

        public virtual ICollection<TransactionAttribute> Attributes { get; set; }

        public virtual ICollection<TransactionWitness> Witnesses { get; set; }

        public virtual ICollection<AddressInTransaction> AddressesInTransactions { get; set; }

        public virtual ICollection<AssetInTransaction> AssetsInTransactions { get; set; }

        public int? EnrollmentTransactionId { get; set; }

        public virtual EnrollmentTransaction EnrollmentTransaction { get; set; }

        public int? InvocationTransactionId { get; set; }

        public virtual InvocationTransaction InvocationTransaction { get; set; }

        public int? MinerTransactionId { get; set; }

        public virtual MinerTransaction MinerTransaction { get; set; }

        public int? PublishTransactionId { get; set; }
        
        public virtual PublishTransaction PublishTransaction { get; set; }

        public int? RegisterTransactionId { get; set; }

        public virtual RegisterTransaction RegisterTransaction { get; set; }

        public int? StateTransactionId { get; set; }

        public virtual StateTransaction StateTransaction { get; set; }
    }
}
