using StateOfNeo.Data.Models.Enums;
using StateOfNeo.Data.Models.Transactions;
using System.Collections.Generic;

namespace StateOfNeo.Data.Models
{
    public class Asset : BaseEntity
    {
        public Asset()
        {
            this.TransactedAssets = new HashSet<TransactedAsset>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Hash { get; set; }

        public GlobalAssetType Type { get; set; }

        public int MaxSupply { get; set; }

        public virtual ICollection<TransactedAsset> TransactedAssets { get; set; }
    }
}
