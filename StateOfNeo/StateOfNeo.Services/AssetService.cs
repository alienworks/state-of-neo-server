using System.Linq;
using AutoMapper.QueryableExtensions;
using StateOfNeo.Common.Enums;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;

namespace StateOfNeo.Services
{
    public class AssetService : IAssetService
    {
        private readonly StateOfNeoContext db;

        public AssetService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public T Find<T>(string hash) =>
            this.db.Assets
                .Where(x => x.Hash == hash)
                .ProjectTo<T>()
                .FirstOrDefault();

        public int Count(AssetType[] types)
        {
            if (types == null || types.Length == 0)
            {
                return this.db.Assets.Count();
            }

            var assets = this.db.Assets.Where(x => types.Contains(x.Type)).Count();
            return assets;
        }

        public int TxCount(AssetType[] types)
        {
            if (types == null || types.Length == 0)
            {
                return this.db.Transactions.Count();
            }

            var assets = this.db.TransactedAssets
                .Where(x => types.Contains(x.AssetType))
                .Select(x => x.AssetType == AssetType.NEP5 ? x.TransactionHash : x.OutGlobalTransactionHash ?? x.InGlobalTransactionHash)
                .Distinct()
                .Count();

            return assets;
        }
    }
}
