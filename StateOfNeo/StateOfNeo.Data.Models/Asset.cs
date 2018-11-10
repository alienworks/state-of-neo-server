using StateOfNeo.Common.Enums;
using StateOfNeo.Data.Models.Transactions;
using System.Collections.Generic;

namespace StateOfNeo.Data.Models
{
    public class Asset : BaseEntity
    {
        public Asset()
        {
            this.Balances = new HashSet<AddressAssetBalance>();
            this.TransactedAssets = new HashSet<TransactedAsset>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Hash { get; set; }

        public int Decimals { get; set; }

        public Neo.Network.P2P.Payloads.AssetType? GlobalType { get; set; }

        public AssetType Type { get; set; }

        public long MaxSupply { get; set; }

        public long CurrentSupply { get; set; }

        public virtual ICollection<AddressAssetBalance> Balances { get; set; }

        public virtual ICollection<TransactedAsset> TransactedAssets { get; set; }
    }
}
