using StateOfNeo.Common.Enums;
using System.Numerics;

namespace StateOfNeo.ViewModels.Asset
{
    public class AssetListViewModel
    {
        public string Name { get; set; }

        public string Hash { get; set; }

        public BigInteger TotalSupply { get; set; }

        public AssetType Type { get; set; }
    }
}
