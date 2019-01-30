using Microsoft.Extensions.Options;
using Neo;
using Neo.Ledger;
using Neo.SmartContract;
using Neo.VM;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data.Models;
using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Enums;

namespace StateOfNeo.Server.Infrastructure
{
    public class BlockchainBalances
    {
        private readonly string connectionString;
        private StateOfNeoContext db;

        public BlockchainBalances(IOptions<DbSettings> dbSettings)
        {
            this.connectionString = dbSettings.Value.DefaultConnection;
            this.db = StateOfNeoContext.Create(this.connectionString);
        }

        public void Run()
        {
            var addresses = this.db.Addresses
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => x.PublicAddress)
                .ToList();
            var contracts = this.db.SmartContracts.ToList();

            var bcAddresses = Blockchain.Singleton.GetSnapshot().Accounts;
            var updatedAddressesCount = 0;

            foreach (var address in addresses)
            {
                var addressUpdated = GetAddressAssets(address, this.db);

                if (addressUpdated != null)
                {
                    updatedAddressesCount++;

                    if (this.db.ChangeTracker.Entries().Count() > 10_000)
                    {
                        Log.Information($"Updated {updatedAddressesCount} out of {addresses.Count}. {addresses.Count - updatedAddressesCount} to go!");
                        this.db.SaveChanges();
                        this.db.Dispose();

                        this.db = StateOfNeoContext.Create(this.connectionString);
                        Log.Information($"Save changes done");
                    }
                }
            }
        }

        public static IDictionary<UInt256, Fixed8> GetGlobalAssets(string address)
        {
            var accHash = address.ToScriptHash();
            var account = Blockchain.Singleton.GetSnapshot().Accounts.TryGet(accHash);

            if (account != null)
            {
                return account.Balances;
            }

            return null;
        }

        static public BigInteger GetTotalSupply(string hash)
        {
            UInt160 script_hash = UInt160.Parse(hash);
            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitAppCall(script_hash, "totalSupply");
                script = sb.ToArray();
            }

            ApplicationEngine engine = ApplicationEngine.Run(script);
            BigInteger amount = engine.ResultStack.Pop().GetBigInteger();

            return amount;
        }

        static public Address GetAddressAssets(string address, StateOfNeoContext db)
        {
            var accHash = address.ToScriptHash();
            var account = Blockchain.Singleton.GetSnapshot().Accounts.TryGet(accHash);

            var assets = db.Assets
                .Where(x => x.Type == AssetType.NEP5)
                .Select(x => x.Hash)
                .ToList();

            var contractsToCheck = db.TransactedAssets
                .Where(x =>
                    (x.FromAddressPublicAddress == address || x.ToAddressPublicAddress == address)
                    && assets.Contains(x.AssetHash))
                .Select(x => x.AssetHash)
                .Distinct()
                .ToList();

            var addressAccount = db.Addresses
                .Include(x => x.Balances).ThenInclude(x => x.Asset)
                .FirstOrDefault(x => x.PublicAddress == address);

            if (addressAccount != null && addressAccount.Balances.Count > 0)
            {
                // Update global assets
                if (account != null)
                {
                    foreach (var accountBalance in addressAccount.Balances.Where(x => x.Asset.Type != AssetType.NEP5))
                    {
                        //var assetBalance = addressAccount.Balances.FirstOrDefault(x => x.AssetHash == balance.Key.ToString());
                        var assetBalance = account.Balances.FirstOrDefault(x => x.Key.ToString() == accountBalance.AssetHash);
                        if (assetBalance.Value != null)
                        {
                            accountBalance.Balance = (decimal)assetBalance.Value;
                        }
                        else
                        {
                            accountBalance.Balance = 0;
                        }
                    }
                }

                foreach (var contract in contractsToCheck)
                {
                    UInt160 script_hash = UInt160.Parse(contract);
                    byte[] script;
                    using (ScriptBuilder sb = new ScriptBuilder())
                    {
                        sb.EmitAppCall(script_hash, "balanceOf", accHash);
                        sb.Emit(OpCode.DEPTH, OpCode.PACK);
                        sb.EmitAppCall(script_hash, "decimals");
                        sb.EmitAppCall(script_hash, "name");
                        script = sb.ToArray();
                    }

                    ApplicationEngine engine = ApplicationEngine.Run(script);
                    if (engine.State.HasFlag(VMState.FAULT)) continue;
                    string name = engine.ResultStack.Pop().GetString();
                    byte decimals = (byte)engine.ResultStack.Pop().GetBigInteger();
                    BigInteger amount = ((Neo.VM.Types.Array)engine.ResultStack.Pop()).Aggregate(BigInteger.Zero, (x, y) => x + y.GetBigInteger());

                    var assetBalance = addressAccount.Balances.FirstOrDefault(x => x.AssetHash == contract);
                    if (assetBalance != null)
                    {
                        assetBalance.Balance = amount.ToDecimal(decimals);
                    }
                    else
                    {
                        assetBalance.Balance = 0;
                    }
                }
            }

            return addressAccount;
        }
    }
}
