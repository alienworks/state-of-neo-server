using Microsoft.EntityFrameworkCore;
using Neo;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Server.Actors.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class BalanceUpdater
    {
        private StateOfNeoContext db;
        private readonly string connectionString;
        private readonly ICollection<Data.Models.SmartContract> pendingSmartContracts = new List<Data.Models.SmartContract>();

        public BalanceUpdater(string connectionString)
        {
            this.connectionString = connectionString;
            this.db = StateOfNeoContext.Create(this.connectionString);

            this.Run();
        }

        public void Run(uint start = 593_072, uint end = 3117429)
        {
            for (uint i = start; i < end; i++)
            {
                var hash = Blockchain.Singleton.GetBlockHash(i);
                var block = Blockchain.Singleton.GetBlock(hash);
                var blockTime = block.Timestamp.ToUnixDate();

                foreach (var transaction in block.Transactions)
                {
                    var transactedAmounts = new Dictionary<string, Dictionary<string, decimal>>();

                    for (int j = 0; j < transaction.Inputs.Length; j++)
                    {
                        var input = transaction.Inputs[j];
                        var fromPublicAddress = transaction.References[input].ScriptHash.ToAddress();
                        var fromAddress = this.GetAddress(fromPublicAddress);
                        fromAddress.LastTransactionOn = blockTime;

                        var amount = (decimal)transaction.References[input].Value;
                        var assetHash = transaction.References[input].AssetId.ToString();
                        var asset = this.GetAsset(assetHash);

                        var ta = this.GetTransactedAsset(fromPublicAddress, transaction.Hash.ToString(), assetHash);
                        ta.Amount = amount;

                        var fromBalance = this.GetBalance(assetHash, fromPublicAddress);
                        fromBalance.Balance -= ta.Amount;

                        this.AdjustTransactedAmount(transactedAmounts, assetHash, fromPublicAddress, -ta.Amount);
                    }

                    for (int j = 0; j < transaction.Outputs.Length; j++)
                    {
                        var output = transaction.Outputs[j];
                        var toPublicAddress = output.ScriptHash.ToAddress();
                        var toAddress = this.GetAddress(toPublicAddress);
                        toAddress.LastTransactionOn = blockTime;

                        var amount = (decimal)output.Value;
                        var assetHash = output.AssetId.ToString();
                        var asset = this.GetAsset(assetHash);

                        var ta = this.GetTransactedAssetTo(toPublicAddress, transaction.Hash.ToString(), assetHash);
                        ta.Amount = amount;

                        var toBalance = this.GetBalance(assetHash, toPublicAddress);
                        toBalance.Balance += ta.Amount;

                        this.AdjustTransactedAmount(transactedAmounts, assetHash, toPublicAddress, ta.Amount);
                    }
                    
                    foreach (var assetTransactions in transactedAmounts)
                    {
                        var asset = this.GetAsset(assetTransactions.Key);
                        asset.TransactionsCount++;

                        var assetInTransaction = this.GetAssetInTransaction(asset.Hash, transaction.Hash.ToString());
                        assetInTransaction.Timestamp = block.Timestamp;

                        foreach (var addressTransaction in assetTransactions.Value)
                        {
                            var address = this.GetAddress(addressTransaction.Key);
                            address.TransactionsCount++;

                            var addressInAssetTransaction = this.GetAddressInAssetTransaction(addressTransaction.Key, assetInTransaction.Id);
                            addressInAssetTransaction.Amount = addressTransaction.Value;

                            var addressInTransaction = this.GetAddressInTransaction(addressTransaction.Key, transaction.Hash.ToString(), asset.Hash);
                            addressInTransaction.Amount = addressTransaction.Value;
                        }
                    }

                    if (transaction.Type == TransactionType.InvocationTransaction)
                    {
                        var unboxed = transaction as InvocationTransaction;
                        this.TrackInvocationTransaction(unboxed, blockTime);
                    }
                }

                if (this.db.ChangeTracker.Entries().Count() > 10_000)
                {
                    this.db.SaveChanges();
                    this.db.Dispose();

                    this.db = StateOfNeoContext.Create(this.connectionString);

                    this.pendingSmartContracts.Clear();
                }
            }

            this.db.SaveChanges();
        }

        private void AdjustTransactedAmount(
            Dictionary<string, Dictionary<string, decimal>> transactedAmounts,
            string assetHash,
            string publicAddress,
            decimal amount)
        {
            if (!transactedAmounts.ContainsKey(assetHash))
            {
                transactedAmounts.Add(assetHash, new Dictionary<string, decimal>());
            }

            if (!transactedAmounts[assetHash].ContainsKey(publicAddress))
            {
                transactedAmounts[assetHash].Add(publicAddress, 0);
            }

            transactedAmounts[assetHash][publicAddress] += amount;
        }

        private Data.Models.AddressInTransaction GetAddressInTransaction(string address, string transactionHash, string assetHash)
        {
            var result = this.db.AddressesInTransactions
                .Where(x => x.AddressPublicAddress == address && x.TransactionHash == transactionHash && x.AssetHash == assetHash)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }

        private Data.Models.AssetInTransaction GetAssetInTransaction(string assetHash, string transactionHash)
        {
            var result = this.db.AssetsInTransactions
                .Where(x => x.AssetHash == assetHash && x.TransactionHash == transactionHash)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }

        private Data.Models.AddressInAssetTransaction GetAddressInAssetTransaction(string address, int assetInTransactionId)
        {
            var result = this.db.AddressesInAssetTransactions
                .Where(x => x.AddressPublicAddress == address && x.AssetInTransactionId == assetInTransactionId)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }
        
        private Data.Models.Transactions.TransactedAsset GetNepTransactedAsset(string fromAddress, string toAddress, string transactionHash, string assetHash)
        {
            var result = this.db.TransactedAssets
                .Where(x => x.AssetHash == assetHash 
                    && x.FromAddressPublicAddress == fromAddress && x.TransactionHash == transactionHash
                    && x.ToAddressPublicAddress == toAddress)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }

        private Data.Models.Transactions.TransactedAsset GetTransactedAsset(string fromAddress, string transactionHash, string assetHash)
        {
            var result = this.db.TransactedAssets
                .Where(x => x.AssetHash == assetHash && x.FromAddressPublicAddress == fromAddress && x.InGlobalTransactionHash == transactionHash)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }

        private Data.Models.Transactions.TransactedAsset GetTransactedAssetTo(string toAddress, string transactionHash, string assetHash)
        {
            var result = this.db.TransactedAssets
                .Where(x => x.AssetHash == assetHash && x.ToAddressPublicAddress == toAddress && x.OutGlobalTransactionHash == transactionHash)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }

        private Data.Models.Asset GetAsset(string hash)
        {
            var result = this.db.Assets
                .Where(x => x.Hash == hash)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }    

        private Data.Models.AddressAssetBalance GetBalance(string hash, string address)
        { 
            var result = this.db.AddressBalances
                .Include(x => x.Asset)
                .Where(x => x.Asset.Hash == hash && x.AddressPublicAddress == address)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }

        private Data.Models.Address GetAddress(string address)
        {
            var result = this.db.Addresses
                .Include(x => x.Balances)
                .ThenInclude(x => x.Asset)
                .Where(x => x.PublicAddress == address)
                .FirstOrDefault();

            if (result == null)
            {

            }

            return result;
        }

        private void EnsureSmartContractCreated(UInt160 contractHash, StateOfNeoContext db, long timestamp)
        {
            var result = db.SmartContracts.Where(x => x.Hash == contractHash.ToString()).FirstOrDefault();
            if (result != null)
            {
                return;
            }

            result = pendingSmartContracts.FirstOrDefault(x => x.Hash == contractHash.ToString());
            if (result != null)
            {
                return;
            }

            var contractsStore = Blockchain.Singleton.Store.GetContracts();
            var sc = contractsStore.TryGet(contractHash);

            var newSc = new StateOfNeo.Data.Models.SmartContract
            {
                Author = sc.Author,
                CreatedOn = DateTime.UtcNow,
                Description = sc.Description,
                Email = sc.Email,
                HasDynamicInvoke = sc.HasDynamicInvoke,
                Hash = contractHash.ToString(),
                HasStorage = sc.HasStorage,
                InputParameters = string.Join(",", sc.ParameterList.Select(x => x)),
                Name = sc.Name,
                Payable = sc.Payable,
                ReturnType = sc.ReturnType,
                Timestamp = timestamp,
                Version = sc.CodeVersion
            };

            db.SmartContracts.Add(newSc);
            pendingSmartContracts.Add(newSc);
        }

        private void TrackInvocationTransaction(InvocationTransaction transaction, DateTime blockTime)
        {
            AppExecutionResult result = null;
            UInt160 contractHash = null;
            using (ApplicationEngine engine = new ApplicationEngine(TriggerType.Application, transaction, Blockchain.Singleton.GetSnapshot().Clone(), transaction.Gas))
            {
                engine.LoadScript(transaction.Script);
                if (engine.Execute())
                {
                    engine.Service.Commit();
                }

                result = new AppExecutionResult
                {
                    Trigger = TriggerType.Application,
                    ScriptHash = transaction.Script.ToScriptHash(),
                    VMState = engine.State,
                    GasConsumed = engine.GasConsumed,
                    Stack = engine.ResultStack.ToArray(),
                    Notifications = engine.Service.Notifications.ToArray()
                };

                contractHash = new UInt160(engine.CurrentContext.ScriptHash);
            }

            //this.EnsureSmartContractCreated(contractHash, db, blockTime.ToUnixTimestamp());

            foreach (var item in result.Notifications)
            {
                var type = item.GetNotificationType();
                string[] notificationStringArray = item.State is Neo.VM.Types.Array ?
                    (item.State as Neo.VM.Types.Array).ToStringList().ToArray() : new string[] { type };

                if (type == "transfer")
                {
                    var name = this.TestInvoke(item.ScriptHash, "name").HexStringToString();
                    var asset = this.GetAsset(contractHash.ToString());
                    var symbol = this.TestInvoke(item.ScriptHash, "symbol").HexStringToString();
                    if (asset == null)
                    {

                        var decimalsHex = this.TestInvoke(item.ScriptHash, "decimals");
                        if (!int.TryParse(decimalsHex, out _))
                        {
                            continue;
                        }

                        var decimals = Convert.ToInt32(decimalsHex, 16);

                        long? totalSupply = null;
                        try
                        {
                            totalSupply = Convert.ToInt64(this.TestInvoke(item.ScriptHash, "totalSupply"), 16);
                        }
                        catch (Exception e)
                        {
                            Log.Warning($"Getting totalSupply throw an error for contract - {contractHash}. In this Max and Total supply are set to null");
                        }

                        asset = new Data.Models.Asset
                        {
                            CreatedOn = DateTime.UtcNow,
                            GlobalType = null,
                            Hash = contractHash.ToString(),
                            Name = name,
                            MaxSupply = totalSupply,
                            Type = StateOfNeo.Common.Enums.AssetType.NEP5,
                            Decimals = decimals,
                            CurrentSupply = totalSupply,
                            Symbol = symbol
                        };

                        db.Assets.Add(asset);
                    }

                    var assetInTransaction = this.GetAssetInTransaction(asset.Hash, transaction.Hash.ToString());
                    assetInTransaction.Timestamp = blockTime.ToUnixTimestamp();

                    //var assetInTransaction = new AssetInTransaction
                    //{
                    //    AssetHash = asset.Hash,
                    //    CreatedOn = DateTime.UtcNow,
                    //    TransactionHash = transaction.Hash.ToString(),
                    //    Timestamp = blockTime.ToUnixTimestamp()
                    //};

                    //db.AssetsInTransactions.Add(assetInTransaction);

                    var isLfx = symbol.ToLower() == "lfx";
                    var notification = isLfx ? item.GetNotification<TransferNotification>(2) : item.GetNotification<TransferNotification>();

                    if (notification.Amount == 0) Log.Warning($"Transfer with 0 amount value or empty array for {name}/{symbol}");
                    if (isLfx) Log.Warning($"Transfer in {name}/{symbol} returns wrong number of arguments {type} - {string.Join(" | ", notificationStringArray)}");

                    string from = null;

                    if (notification.From.Length == 20)
                    {
                        from = new UInt160(notification.From).ToAddress();
                    }

                    string to = null;

                    if (notification.To.Length != 20)
                    {
                        Log.Warning($"{item.ScriptHash} NEP-5 token {name} / {symbol} invalid To address. Tx {transaction.Hash}");
                    }
                    else
                    {
                        to = new UInt160(notification.To).ToAddress();
                    }

                    var ta = this.GetNepTransactedAsset(from, to, transaction.Hash.ToString(), asset.Hash);
                    ta.Amount = notification.Amount.ToDecimal(asset.Decimals);
                    //var ta = new Data.Models.Transactions.TransactedAsset
                    //{
                    //    Amount = notification.Amount.ToDecimal(asset.Decimals),
                    //    Asset = asset,
                    //    FromAddressPublicAddress = from,
                    //    ToAddressPublicAddress = to,
                    //    AssetType = AssetType.NEP5,
                    //    CreatedOn = DateTime.UtcNow,
                    //    TransactionHash = transaction.Hash.ToString()
                    //};

                    //db.TransactedAssets.Add(ta);

                    if (from != null)
                    {
                        var fromAddress = this.GetAddress(from);
                        fromAddress.LastTransactionOn = blockTime;
                        fromAddress.TransactionsCount++;

                        var fromAddressInTransaction = this.GetAddressInTransaction(fromAddress.PublicAddress, ta.TransactionHash, asset.Hash);
                        fromAddressInTransaction.Amount = ta.Amount;
                        fromAddressInTransaction.Timestamp = blockTime.ToUnixTimestamp();
                        //var fromAddressInTransaction = new AddressInTransaction
                        //{
                        //    AddressPublicAddress = fromAddress.PublicAddress,
                        //    Amount = ta.Amount,
                        //    AssetHash = asset.Hash,
                        //    CreatedOn = DateTime.UtcNow,
                        //    Timestamp = blockTime.ToUnixTimestamp(),
                        //    TransactionHash = ta.TransactionHash
                        //};

                        var fromAddressInAssetTransaction = this.GetAddressInAssetTransaction(fromAddress.PublicAddress, assetInTransaction.Id);
                        fromAddressInAssetTransaction.Amount = ta.Amount;
                        //var fromAddressInAssetTransaction = new AddressInAssetTransaction
                        //{
                        //    AddressPublicAddress = fromAddress.PublicAddress,
                        //    CreatedOn = DateTime.UtcNow,
                        //    Amount = ta.Amount
                        //};

                        //assetInTransaction.AddressesInAssetTransactions.Add(fromAddressInAssetTransaction);
                    }

                    if (to != null)
                    {
                        var toAddress = this.GetAddress(to);
                        toAddress.LastTransactionOn = blockTime;
                        toAddress.TransactionsCount++;

                        var toAddressInTransaction = this.GetAddressInTransaction(toAddress.PublicAddress, ta.TransactionHash, asset.Hash);
                        toAddressInTransaction.Amount = ta.Amount;
                        toAddressInTransaction.Timestamp = blockTime.ToUnixTimestamp();
                        //var toAddressInTransaction = new AddressInTransaction
                        //{
                        //    AddressPublicAddress = toAddress.PublicAddress,
                        //    Amount = ta.Amount,
                        //    AssetHash = asset.Hash,
                        //    CreatedOn = DateTime.UtcNow,
                        //    Timestamp = blockTime.ToUnixTimestamp(),
                        //    TransactionHash = ta.TransactionHash
                        //};

                        var toAddressInAssetTransaction = this.GetAddressInAssetTransaction(toAddress.PublicAddress, assetInTransaction.Id);
                        toAddressInAssetTransaction.Amount = ta.Amount;
                        //var toAddressInAssetTransaction = new AddressInAssetTransaction
                        //{
                        //    AddressPublicAddress = toAddress.PublicAddress,
                        //    CreatedOn = DateTime.UtcNow,
                        //    Amount = ta.Amount
                        //};

                        //assetInTransaction.AddressesInAssetTransactions.Add(toAddressInAssetTransaction);
                    }

                    asset.TransactionsCount++;

                    var fromBalance = this.GetBalance(asset.Hash, from);
                    fromBalance.TransactionsCount++;
                    fromBalance.Balance -= ta.Amount;
                    if (fromBalance.Balance < 0)
                    {
                        fromBalance.Balance = -1;
                    }

                    var toBalance = this.GetBalance(asset.Hash, to);
                    toBalance.TransactionsCount++;
                    toBalance.Balance += ta.Amount;
                }
                else
                {
                    Log.Information($@"Notification of type - {type} has been thrown by contract - {contractHash}
                        This is for tx = {transaction.Hash.ToString()}");
                }
            }
        }

        private string TestInvoke(UInt160 contractHash, string operation, params object[] args)
        {
            var result = this.TestInvokeForStackItem(contractHash, operation, args);
            if (result == null)
            {
                return "";
            }

            return result.GetByteArray().ToHexString();
        }

        private StackItem TestInvokeForStackItem(UInt160 contractHash, string operation, params object[] args)
        {
            var sb = new ScriptBuilder();
            var parameters = new ContractParameter[]
            {
                    new ContractParameter { Type = ContractParameterType.String, Value = operation },
                    new ContractParameter { Type = ContractParameterType.Array, Value = new ContractParameter[0] }
            };

            sb.EmitAppCall(contractHash, parameters);

            var script = sb.ToArray();
            var engine = ApplicationEngine.Run(script, testMode: true);
            var result = engine.ResultStack.FirstOrDefault();

            return result;
        }
    }
}
