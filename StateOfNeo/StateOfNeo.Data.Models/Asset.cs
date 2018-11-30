using StateOfNeo.Common.Enums;
using StateOfNeo.Data.Models.Transactions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class Asset : BaseEntity
    {
        public Asset()
        {
            this.AddressesInTransactions = new HashSet<AddressInTransaction>();
            this.AssetsInTransactions = new HashSet<AssetInTransaction>();
            this.Balances = new HashSet<AddressAssetBalance>();
            this.TransactedAssets = new HashSet<TransactedAsset>();
        }

        [Key]
        public string Hash { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public int Decimals { get; set; }

        public Neo.Network.P2P.Payloads.AssetType? GlobalType { get; set; }

        public AssetType Type { get; set; }

        public long? MaxSupply { get; set; }

        public long? CurrentSupply { get; set; }

        public int TransactionsCount { get; set; }

        public virtual ICollection<AddressAssetBalance> Balances { get; set; }

        public virtual ICollection<TransactedAsset> TransactedAssets { get; set; }

        public virtual ICollection<AddressInTransaction> AddressesInTransactions { get; set; }

        public virtual ICollection<AssetInTransaction> AssetsInTransactions { get; set; }
    }
}
