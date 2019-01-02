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
                .Skip(999674)
                .Select(x => x.PublicAddress)
                .ToList();
            var contracts = this.db.SmartContracts.ToList();

            var assets = this.db.Assets
                .Where(x => x.Type == StateOfNeo.Common.Enums.AssetType.NEP5)
                .Select(x => x.Hash)
                .ToList();

            var bcAddresses = Blockchain.Singleton.GetSnapshot().Accounts;
            var updatedAddressesCount = 0;

            foreach (var address in addresses)
            {
                var accHash = address.ToScriptHash();
                //var accHash = "AKJQMHma9MA8KK5M8iQg8ASeg3KZLsjwvB".ToScriptHash();
                var account = Blockchain.Singleton.GetSnapshot().Accounts.TryGet(accHash);

                var contractsToCheck = this.db.TransactedAssets
                    .Where(x =>
                        (x.FromAddressPublicAddress == address || x.ToAddressPublicAddress == address)
                        //(x.FromAddressPublicAddress == "AKJQMHma9MA8KK5M8iQg8ASeg3KZLsjwvB" || x.ToAddressPublicAddress == "AKJQMHma9MA8KK5M8iQg8ASeg3KZLsjwvB")
                        && assets.Contains(x.AssetHash))
                    .Select(x => x.AssetHash)
                    .Distinct()
                    .ToList();

                //var addressAccount = this.db.Addresses.FirstOrDefault(x=>x.PublicAddress == address);
                var addressAccount = this.db.Addresses
                    .Include(x => x.Balances)
                    .FirstOrDefault(x => x.PublicAddress == address);
                //.FirstOrDefault(x => x.PublicAddress == "AKJQMHma9MA8KK5M8iQg8ASeg3KZLsjwvB");

                if (addressAccount != null && addressAccount.Balances.Count > 0)
                {
                    // Update global assets
                    if (account != null)
                    {
                        foreach (var balance in account.Balances)
                        {
                            var assetBalance = addressAccount.Balances.FirstOrDefault(x => x.AssetHash == balance.Key.ToString());
                            if (assetBalance != null)
                            {
                                assetBalance.Balance = (decimal)balance.Value;
                                this.db.AddressBalances.Update(assetBalance);
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
                            this.db.AddressBalances.Update(assetBalance);
                        }

                    }
                }

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
}
