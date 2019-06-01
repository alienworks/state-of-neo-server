using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Wallets;
using StateOfNeo.Common;
using StateOfNeo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class AssetsCreatorUpdate
    {
        private readonly string connectionString;

        public AssetsCreatorUpdate(IOptions<DbSettings> dbSettings)
        {
            this.connectionString = dbSettings.Value.DefaultConnection;
        }

        public async Task Run()
        {
            var db = StateOfNeoContext.Create(this.connectionString);

            // NEO
            var neoAsset = db.Assets.FirstOrDefault(x => x.Name == "NEO");

            if (neoAsset != null)
            {
                neoAsset.CreatorAddressId = Blockchain.GoverningToken.Admin.ToAddress();
                db.Assets.Update(neoAsset);
            }

            // GAS
            var gasAsset = db.Assets.FirstOrDefault(x => x.Name == "GAS");

            if (gasAsset != null)
            {
                gasAsset.CreatorAddressId = Blockchain.UtilityToken.Admin.ToAddress();
                db.Assets.Update(gasAsset);
            }

            // GlobalAssets
            var globalAssets = db.Assets
                .Where(x => x.GlobalType.HasValue)
                .Where(x => x.CreatorAddressId != null)
                .ToList();

            var registerTransactions = db.RegisterTransactions.ToList();

            foreach (var regTx in registerTransactions)
            {
                var jsonName = JObject.Parse(regTx.Name.Substring(1, regTx.Name.Length - 2));
                var name = jsonName["name"]?.AsString();

                var asset = globalAssets.FirstOrDefault(x => x.Name == name);

                if (asset != null)
                {
                    asset.CreatorAddressId = regTx.AdminAddress;
                    db.Assets.Update(asset);
                }
            }

            Nep5CreatorUpdate(db);

            await db.SaveChangesAsync();
        }

        public static void Nep5CreatorUpdate(StateOfNeoContext db)
        {
            var scs = db.SmartContracts.ToList();
            var nepAssets = db.Assets
                .Where(x => x.CreatorAddressId == null)
                .Where(x => !x.GlobalType.HasValue)
                .Where(x => x.Type == StateOfNeo.Common.Enums.AssetType.NEP5)
                .ToList();

            var nepScAssets = nepAssets
                .Select(x =>
                {
                    var sc = scs.FirstOrDefault(y => y.Hash == x.Hash);
                    return new
                    {
                        Asset = x,
                        SmartContract = sc
                    };
                })
                .ToList();

            for (int i = 0; i < nepScAssets.Count; i++)
            {
                var assetContract = nepScAssets[i];

                var testTxId = db.InvocationTransactions
                    .FirstOrDefault(x => x.SmartContractId == assetContract.SmartContract.Id);

                var addressInTransactions = db.AddressesInTransactions
                    .Include(x => x.Transaction)
                    .Where(x => x.TransactionHash == testTxId.TransactionHash)
                    .ToList();

                var creatorAddress = addressInTransactions.Count > 1 ?
                    addressInTransactions.FirstOrDefault(x => x.Amount <= 0) :
                    addressInTransactions.FirstOrDefault();

                if (creatorAddress != null)
                {
                    var asset = assetContract.Asset;
                    asset.CreatorAddressId = creatorAddress.AddressPublicAddress;
                    db.Assets.Update(asset);
                }
            }

            db.SaveChanges();
        }
    }
}
