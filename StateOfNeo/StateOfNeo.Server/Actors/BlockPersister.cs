using Akka.Actor;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Neo;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Transactions;
using StateOfNeo.Server.Actors.Notifications;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.Services;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Address;
using StateOfNeo.ViewModels.Transaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Neo.Ledger.Blockchain;

namespace StateOfNeo.Server.Actors
{
    public class BlockPersister : UntypedActor
    {
        private readonly Dictionary<string, string> hashesByNet = new Dictionary<string, string>()
        {
            { "genesisBlock-PrivNet", "0x996e37358dc369912041f966f8c5d8d3a8255ba5dcbd3447f8a82b55db869099" },
            { "genesisBlock-TestNet", "0xb3181718ef6167105b70920e4a8fbbd0a0a56aacf460d70e10ba6fa1668f1fef" },
            { "genesisBlock-MainNet", "0xd42561e3d30e15be6400b6df2f328e02d2bf6354c41dce433bc57687c82144bf" },
            { "neoIssue-PrivNet", "0x7aadf91ca8ac1e2c323c025a7e492bee2dd90c783b86ebfc3b18db66b530a76d" },
            { "neoIssue-TestNet", "0xbdecbb623eee6f9ade28d5a8ff5fb3ea9c9d73af039e0286201b3b0291fb4d4a" },
            { "neoIssue-MainNet", "0x3631f66024ca6f5b033d7e0809eb993443374830025af904fb51b0334f127cda" },
        };

        private readonly string connectionString;
        private readonly string net;
        private readonly IHubContext<StatsHub> statsHub;
        private readonly IHubContext<TransactionsHub> txHub;
        private readonly IHubContext<NotificationHub> notificationHub;
        private IStateService state;
        private readonly BlockchainBalances blockchainBalancesRecalculator;
        private DateTime genesisTime = GenesisBlock.Timestamp.ToUnixDate();
        private bool balancesRecalculated = false;

        private readonly ICollection<Data.Models.Asset> pendingAssets = new List<Data.Models.Asset>();
        private readonly ICollection<Data.Models.SmartContract> pendingSmartContracts = new List<Data.Models.SmartContract>();
        private readonly ICollection<Data.Models.Address> pendingAddresses = new List<Data.Models.Address>();
        private readonly ICollection<Data.Models.AddressAssetBalance> pendingBalances = new List<Data.Models.AddressAssetBalance>();
        private readonly ICollection<ConsensusNode> pendingConsensusNodes = new List<ConsensusNode>();

        public BlockPersister(
            IActorRef blockchain,
            string connectionString,
            IStateService state,
            IHubContext<StatsHub> statsHub,
            IHubContext<TransactionsHub> txHub,
            IHubContext<NotificationHub> notificationHub,
            BlockchainBalances blockChainBalancesRecalculator,
            string net)
        {
            this.connectionString = connectionString;
            this.statsHub = statsHub;
            this.txHub = txHub;
            this.notificationHub = notificationHub;
            this.net = net;
            this.state = state;
            this.blockchainBalancesRecalculator = blockChainBalancesRecalculator;

            blockchain.Tell(new Register());
        }

        public static Props Props(
            IActorRef blockchain,
            string connectionString,
            IStateService state,
            IHubContext<StatsHub> statsHub,
            IHubContext<TransactionsHub> txHub,
            IHubContext<NotificationHub> notificationHub,
            BlockchainBalances blockChainBalances,
            string net) =>
                Akka.Actor.Props.Create(
                    () => new BlockPersister(
                        blockchain,
                        connectionString,
                        state,
                        statsHub,
                        txHub,
                        notificationHub,
                        blockChainBalances,
                        net));

        protected override void OnReceive(object message)
        {
            if (message is PersistCompleted m)
            {
                if (m.Block.Index <= 3062777)
                {
                    return;
                }

                var db = StateOfNeoContext.Create(this.connectionString);
                if (db.Blocks.Any(x => x.Hash == m.Block.Hash.ToString()))
                {
                    return;
                }

                if (!db.Blocks.Any(x => x.Hash == this.hashesByNet["genesisBlock-" + this.net]))
                {
                    this.SeedGenesisBlock(db);
                }

                var currentHeight = db.Blocks.OrderByDescending(x => x.Height).Select(x => x.Height).FirstOrDefault();

                Block persisted = null;
                Neo.Network.P2P.Payloads.Block block = null;
                while (currentHeight < m.Block.Index)
                {
                    var hash = Blockchain.Singleton.GetBlockHash((uint)currentHeight + 1);

                    // Left for duplicate transaction hash issue
                    //var hash = Blockchain.Singleton.GetBlockHash((uint)2000357); //1826259
                    //var hash1 = Blockchain.Singleton.GetBlockHash((uint)1826259); //

                    block = Blockchain.Singleton.GetBlock(hash);
                    persisted = this.PersistBlock(block, db);
                    currentHeight++;
                    if (db.ChangeTracker.Entries().Count() > 10_000)
                    {
                        this.SaveEmitAndClear(db, persisted, block.Transactions.Length);

                        db = StateOfNeoContext.Create(this.connectionString);
                    }
                }

                this.SaveEmitAndClear(db, persisted, block.Transactions.Length);

                db.Dispose();
            }
        }

        private Block PersistBlock(Neo.Network.P2P.Payloads.Block blockToPersist, StateOfNeoContext db)
        {
            var previousHash = Blockchain.Singleton.GetBlockHash(blockToPersist.Header.Index - 1);
            var previousBlock = Blockchain.Singleton.GetBlock(previousHash);

            var block = new Block
            {
                Hash = blockToPersist.Hash.ToString(),
                Height = (int)blockToPersist.Header.Index,
                Size = blockToPersist.Size,
                Timestamp = blockToPersist.Timestamp,
                Validator = previousBlock.NextConsensus.ToAddress(),
                CreatedOn = DateTime.UtcNow,
                ConsensusData = blockToPersist.ConsensusData,
                InvocationScript = blockToPersist.Witness.InvocationScript.ToHexString(),
                VerificationScript = blockToPersist.Witness.VerificationScript.ToHexString(),
                NextConsensusNodeAddress = blockToPersist.NextConsensus.ToAddress(),
                PreviousBlockHash = blockToPersist.PrevHash.ToString(),
                TimeInSeconds = 20
            };

            var blockTime = block.Timestamp.ToUnixDate();

            if (block.Height > 0)
            {
                var previousBlockTime = previousBlock.Timestamp.ToUnixDate();
                block.TimeInSeconds = (blockTime - previousBlockTime).TotalSeconds;
            }

            db.Blocks.Add(block);

            var consensusNode = this.GetConsensusNode(db, block.Validator);

            foreach (var item in blockToPersist.Transactions)
            {
                var newTxHash = item.Hash.ToString();
                while (db.Transactions.Any(x => x.Hash == newTxHash))
                {
                    newTxHash += "+1";
                    Log.Warning($"Duplicate transaction hash - {newTxHash}");
                }

                var transaction = new Transaction
                {
                    Type = item.Type,
                    Hash = newTxHash,
                    CreatedOn = DateTime.UtcNow,
                    NetworkFee = (decimal)item.NetworkFee,
                    SystemFee = (decimal)item.SystemFee,
                    Size = item.Size,
                    Version = item.Version,
                    Timestamp = block.Timestamp
                };

                consensusNode.CollectedFees += transaction.NetworkFee;
                block.Transactions.Add(transaction);

                foreach (var attribute in item.Attributes)
                {
                    var ta = new TransactionAttribute
                    {
                        CreatedOn = DateTime.Now,
                        DataAsHexString = attribute.Data.ToHexString(),
                        Usage = (int)attribute.Usage
                    };

                    transaction.Attributes.Add(ta);
                }

                foreach (var witness in item.Witnesses)
                {
                    transaction.Witnesses.Add(new TransactionWitness
                    {
                        CreatedOn = DateTime.UtcNow,
                        Address = witness.ScriptHash.ToAddress(),
                        InvocationScriptAsHexString = witness.InvocationScript.ToHexString(),
                        VerificationScriptAsHexString = witness.VerificationScript.ToHexString()
                    });
                }

                if (item.Type == Neo.Network.P2P.Payloads.TransactionType.MinerTransaction)
                {
                    var unboxed = item as Neo.Network.P2P.Payloads.MinerTransaction;
                    var minerTransaction = new MinerTransaction { Nonce = unboxed.Nonce };
                    transaction.MinerTransaction = minerTransaction;

                    if (item.Outputs.Any())
                    {
                        var sum = item.Outputs.Sum(x => (decimal)x.Value);
                        this.state.AddConsensusRewards(sum, blockTime);
                    }
                }
                else if (item.Type == Neo.Network.P2P.Payloads.TransactionType.RegisterTransaction)
                {
                    var unboxed = item as Neo.Network.P2P.Payloads.RegisterTransaction;
                    var registerTransaction = new RegisterTransaction
                    {
                        AdminAddress = unboxed.Admin.ToAddress().ToString(),
                        Amount = (decimal)unboxed.Amount,
                        AssetType = unboxed.AssetType,
                        CreatedOn = System.DateTime.UtcNow,
                        Name = unboxed.Name,
                        OwnerPublicKey = unboxed.Owner.ToString(),
                        Precision = unboxed.Precision
                    };

                    transaction.RegisterTransaction = registerTransaction;

                    var asset = new Asset
                    {
                        CreatedOn = DateTime.UtcNow,
                        Name = unboxed.Name,
                        MaxSupply = (long)unboxed.Amount,
                        GlobalType = unboxed.AssetType,
                        Type = AssetType.OTHER,
                        Hash = item.Hash.ToString(),
                        Decimals = unboxed.Precision
                    };

                    if (unboxed.Name.Length > 2)
                    {
                        var jsonName = JObject.Parse(unboxed.Name.Substring(1, unboxed.Name.Length - 2));
                        var name = jsonName["name"]?.AsString();
                        asset.Name = name;
                    }

                    db.Assets.Add(asset);
                    pendingAssets.Add(asset);
                    this.state.MainStats.AddTotalAssetsCount(1);
                }
                else if (item.Type == Neo.Network.P2P.Payloads.TransactionType.EnrollmentTransaction)
                {
                    var unboxed = item as Neo.Network.P2P.Payloads.EnrollmentTransaction;
                    var enrollmentTransaction = new EnrollmentTransaction { PublicKey = unboxed.PublicKey.ToString() };
                    transaction.EnrollmentTransaction = enrollmentTransaction;
                }
                else if (item.Type == Neo.Network.P2P.Payloads.TransactionType.InvocationTransaction)
                {
                    var unboxed = item as Neo.Network.P2P.Payloads.InvocationTransaction;
                    var invocationTransaction = new InvocationTransaction
                    {
                        CreatedOn = System.DateTime.UtcNow,
                        Gas = (decimal)unboxed.Gas,
                        ScriptAsHexString = unboxed.Script.ToHexString()
                    };

                    this.TrackInvocationTransaction(unboxed, db, block.Timestamp.ToUnixDate());

                    transaction.InvocationTransaction = invocationTransaction;
                }
                else if (item.Type == Neo.Network.P2P.Payloads.TransactionType.PublishTransaction)
                {
                    var unboxed = item as Neo.Network.P2P.Payloads.PublishTransaction;
                    var publishTransaction = new PublishTransaction
                    {
                        CreatedOn = DateTime.UtcNow,
                        Author = unboxed.Author,
                        CodeVersion = unboxed.CodeVersion,
                        Description = unboxed.Description,
                        Email = unboxed.Email,
                        Name = unboxed.Name,
                        NeedStorage = unboxed.NeedStorage,
                        ParameterList = string.Join("", unboxed.ParameterList.Select(x => x.ToString())),
                        ReturnType = unboxed.ReturnType.ToString(),
                        ScriptAsHexString = unboxed.Script.ToHexString()
                    };

                    transaction.PublishTransaction = publishTransaction;
                }
                else if (item.Type == Neo.Network.P2P.Payloads.TransactionType.StateTransaction)
                {
                    var unboxed = item as Neo.Network.P2P.Payloads.StateTransaction;
                    var stateTransaction = new StateTransaction
                    {
                        CreatedOn = DateTime.UtcNow
                    };

                    foreach (var sd in unboxed.Descriptors)
                    {
                        stateTransaction.Descriptors.Add(new StateDescriptor
                        {
                            CreatedOn = DateTime.UtcNow,
                            Field = sd.Field,
                            KeyAsHexString = sd.Key.ToHexString(),
                            ValueAsHexString = sd.Value.ToHexString(),
                            Type = sd.Type
                        });
                    }

                    transaction.StateTransaction = stateTransaction;
                }

                var transactedAmounts = new Dictionary<string, Dictionary<string, decimal>>();
                var activeAddresses = new List<AddressListViewModel>();
                for (int i = 0; i < item.Inputs.Length; i++)
                {
                    var input = item.Inputs[i];
                    var fromPublicAddress = item.References[input].ScriptHash.ToAddress();
                    var fromAddress = this.GetAddress(db, fromPublicAddress, blockTime);

                    var amount = (decimal)item.References[input].Value;
                    var assetHash = item.References[input].AssetId.ToString();
                    var asset = this.GetAsset(db, assetHash);

                    var ta = new TransactedAsset
                    {
                        Amount = amount,
                        FromAddress = fromAddress,
                        AssetType = assetHash == AssetConstants.NeoAssetId ? AssetType.NEO : AssetType.GAS,
                        CreatedOn = DateTime.UtcNow,
                        AssetHash = asset.Hash,
                        FromAddressPublicAddress = fromPublicAddress
                    };

                    fromAddress.LastTransactionOn = blockTime;
                    fromAddress.LastTransactionStamp = block.Timestamp;
                    var fromBalance = this.GetBalance(db, asset.Hash, fromAddress.PublicAddress);
                    fromBalance.Balance -= ta.Amount;
                    this.AdjustTransactedAmount(transactedAmounts, assetHash, fromPublicAddress, -ta.Amount);

                    transaction.GlobalIncomingAssets.Add(ta);

                    var activeAddress = Mapper.Map<AddressListViewModel>(fromAddress);
                    activeAddresses.Add(activeAddress);
                }

                for (int i = 0; i < item.Outputs.Length; i++)
                {
                    var output = item.Outputs[i];
                    var toPublicAddress = output.ScriptHash.ToAddress();
                    var toAddress = this.GetAddress(db, toPublicAddress, blockTime);

                    var amount = (decimal)output.Value;
                    var assetHash = output.AssetId.ToString();
                    var asset = this.GetAsset(db, assetHash);

                    var ta = new TransactedAsset
                    {
                        Amount = amount,
                        ToAddress = toAddress,
                        AssetType = assetHash == AssetConstants.NeoAssetId ? AssetType.NEO : AssetType.GAS,
                        CreatedOn = DateTime.UtcNow,
                        AssetHash = asset.Hash,
                        ToAddressPublicAddress = toPublicAddress
                    };

                    toAddress.LastTransactionOn = blockTime;
                    toAddress.LastTransactionStamp = block.Timestamp;
                    var toBalance = this.GetBalance(db, asset.Hash, toAddress.PublicAddress);
                    toBalance.Balance += ta.Amount;
                    this.AdjustTransactedAmount(transactedAmounts, assetHash, toPublicAddress, ta.Amount);

                    transaction.GlobalOutgoingAssets.Add(ta);
                    transaction.Assets.Add(ta);

                    var activeAddress = Mapper.Map<AddressListViewModel>(toAddress);
                    activeAddresses.Add(activeAddress);

                    if (transaction.Type == Neo.Network.P2P.Payloads.TransactionType.ClaimTransaction)
                    {
                        this.state.MainStats.AddTotalClaimed(ta.Amount);
                    }
                }

                foreach (var assetTransactions in transactedAmounts)
                {
                    var asset = this.GetAsset(db, assetTransactions.Key);
                    asset.TransactionsCount++;

                    var assetInTransaction = new AssetInTransaction
                    {
                        AssetHash = assetTransactions.Key,
                        TransactionHash = transaction.Hash,
                        CreatedOn = DateTime.UtcNow,
                        Timestamp = transaction.Timestamp
                    };

                    transaction.AssetsInTransactions.Add(assetInTransaction);
                    this.state.MainStats.AddToTotalNeoGasTxCount(1);

                    foreach (var addressTransaction in assetTransactions.Value)
                    {
                        var address = this.GetAddress(db, addressTransaction.Key, blockTime);
                        address.TransactionsCount++;

                        var addressInAssetTransaction = new AddressInAssetTransaction
                        {
                            AddressPublicAddress = addressTransaction.Key,
                            CreatedOn = DateTime.UtcNow,
                            Amount = addressTransaction.Value
                        };

                        assetInTransaction.AddressesInAssetTransactions.Add(addressInAssetTransaction);

                        var addressInTransaction = new AddressInTransaction
                        {
                            AddressPublicAddress = addressTransaction.Key,
                            AssetHash = assetTransactions.Key,
                            TransactionHash = transaction.Hash,
                            Amount = addressTransaction.Value,
                            CreatedOn = DateTime.UtcNow,
                            Timestamp = block.Timestamp
                        };

                        transaction.AddressesInTransactions.Add(addressInTransaction);
                    }
                }

                block.Transactions.Add(transaction);
                this.state.AddActiveAddress(activeAddresses);
            }

            this.state.AddBlockSize(block.Size, blockTime);
            this.state.AddBlockTime(block.TimeInSeconds, blockTime);
            this.state.AddTransactions(blockToPersist.Transactions.Length, blockTime);

            return block;
        }

        private void EnsureSmartContractCreated(UInt160 contractHash, StateOfNeoContext db, long timestamp)
        {
            if (pendingSmartContracts.Any(x => x.Hash == contractHash.ToString())
                || db.SmartContracts.Any(x => x.Hash == contractHash.ToString()))
            {
                return;
            }

            var contractsStore = Singleton.Store.GetContracts();
            var sc = contractsStore.TryGet(contractHash);
            if (sc == null)
            {
                Log.Information($"Tryed to create not existing contract with hash: {contractHash}. Timestamp: {timestamp}");
                return;
            }

            var newSc = new SmartContract
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

        private void TrackInvocationTransaction(Neo.Network.P2P.Payloads.InvocationTransaction transaction, StateOfNeoContext db, DateTime blockTime)
        {
            AppExecutionResult result = null;
            using (ApplicationEngine engine = new ApplicationEngine(TriggerType.Application, transaction, Blockchain.Singleton.GetSnapshot().Clone(), transaction.Gas, true))
            {
                engine.LoadScript(transaction.Script);
                while (
                    !engine.State.HasFlag(VMState.FAULT)
                    && engine.InvocationStack.Any()
                    && engine.CurrentContext.InstructionPointer != engine.CurrentContext.Script.Length)
                {
                    var nextOpCode = engine.CurrentContext.NextInstruction;
                    if (nextOpCode == OpCode.APPCALL)
                    {
                        var startingPosition = engine.CurrentContext.InstructionPointer;
                        engine.CurrentContext.InstructionPointer = startingPosition + 1;

                        var reader = engine.CurrentContext.GetFieldValue<BinaryReader>("OpReader");
                        var rawContractHash = reader.ReadBytes(20);
                        if (rawContractHash.All(x => x == 0))
                        {
                            rawContractHash = engine.CurrentContext.EvaluationStack.Pop().GetByteArray();
                        }

                        engine.CurrentContext.InstructionPointer = startingPosition;

                        var contractHash = new UInt160(rawContractHash);
                        this.EnsureSmartContractCreated(contractHash, db, blockTime.ToUnixTimestamp());
                    }

                    engine.StepInto();
                }

                var success = !engine.State.HasFlag(VMState.FAULT);
                if (success)
                {
                    engine.Service.Commit();

                    var createdContracts = engine.Service
                        .GetFieldValue<Dictionary<UInt160, UInt160>>("ContractsCreated")
                        .Select(x => x.Key)
                        .ToList();

                    foreach (var item in createdContracts)
                    {
                        this.EnsureSmartContractCreated(item, db, blockTime.ToUnixTimestamp());
                    }
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
            }

            foreach (var item in result.Notifications)
            {
                var type = item.GetNotificationType();
                string[] notificationStringArray = item.State is Neo.VM.Types.Array
                    ? (item.State as Neo.VM.Types.Array).ToStringList().ToArray()
                    : new string[] { type };

                if (type == "transfer")
                {
                    var name = this.TestInvoke(item.ScriptHash, "name").HexStringToString();
                    var asset = this.GetAsset(db, item.ScriptHash.ToString());
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
                            Log.Warning($"Getting totalSupply throw an error for contract - {item.ScriptHash.ToString()}. In this Max and Total supply are set to null");
                        }

                        asset = new Asset
                        {
                            CreatedOn = DateTime.UtcNow,
                            GlobalType = null,
                            Hash = item.ScriptHash.ToString(),
                            Name = name,
                            MaxSupply = totalSupply,
                            Type = AssetType.NEP5,
                            Decimals = decimals,
                            CurrentSupply = totalSupply,
                            Symbol = symbol
                        };

                        db.Assets.Add(asset);
                        this.pendingAssets.Add(asset);
                        this.state.MainStats.AddTotalAssetsCount(1);
                    }

                    var assetInTransaction = new AssetInTransaction
                    {
                        AssetHash = asset.Hash,
                        CreatedOn = DateTime.UtcNow,
                        TransactionHash = transaction.Hash.ToString(),
                        Timestamp = blockTime.ToUnixTimestamp()
                    };

                    db.AssetsInTransactions.Add(assetInTransaction);
                    this.state.MainStats.AddToTotalNep5TxCount(1);

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

                    var ta = new Data.Models.Transactions.TransactedAsset
                    {
                        Amount = notification.Amount.ToDecimal(asset.Decimals),
                        Asset = asset,
                        FromAddressPublicAddress = from,
                        ToAddressPublicAddress = to,
                        AssetType = AssetType.NEP5,
                        CreatedOn = DateTime.UtcNow,
                        TransactionHash = transaction.Hash.ToString()
                    };

                    db.TransactedAssets.Add(ta);

                    if (from != null)
                    {
                        var fromAddress = this.GetAddress(db, from, blockTime);
                        fromAddress.LastTransactionOn = blockTime;
                        fromAddress.LastTransactionStamp = blockTime.ToUnixTimestamp();
                        fromAddress.TransactionsCount++;

                        var fromAddressInTransaction = new AddressInTransaction
                        {
                            AddressPublicAddress = fromAddress.PublicAddress,
                            Amount = ta.Amount,
                            AssetHash = asset.Hash,
                            CreatedOn = DateTime.UtcNow,
                            Timestamp = blockTime.ToUnixTimestamp(),
                            TransactionHash = ta.TransactionHash
                        };

                        db.AddressesInTransactions.Add(fromAddressInTransaction);

                        var fromAddressInAssetTransaction = new AddressInAssetTransaction
                        {
                            AddressPublicAddress = fromAddress.PublicAddress,
                            CreatedOn = DateTime.UtcNow,
                            Amount = ta.Amount
                        };

                        assetInTransaction.AddressesInAssetTransactions.Add(fromAddressInAssetTransaction);
                    }

                    if (to != null)
                    {
                        var toAddress = this.GetAddress(db, to, blockTime);
                        toAddress.LastTransactionOn = blockTime;
                        toAddress.LastTransactionStamp = blockTime.ToUnixTimestamp();
                        toAddress.TransactionsCount++;

                        var toAddressInTransaction = new AddressInTransaction
                        {
                            AddressPublicAddress = toAddress.PublicAddress,
                            Amount = ta.Amount,
                            AssetHash = asset.Hash,
                            CreatedOn = DateTime.UtcNow,
                            Timestamp = blockTime.ToUnixTimestamp(),
                            TransactionHash = ta.TransactionHash
                        };

                        db.AddressesInTransactions.Add(toAddressInTransaction);

                        var toAddressInAssetTransaction = new AddressInAssetTransaction
                        {
                            AddressPublicAddress = toAddress.PublicAddress,
                            CreatedOn = DateTime.UtcNow,
                            Amount = ta.Amount
                        };

                        assetInTransaction.AddressesInAssetTransactions.Add(toAddressInAssetTransaction);
                    }

                    db.AssetsInTransactions.Add(assetInTransaction);

                    asset.TransactionsCount++;

                    var fromBalance = this.GetBalance(db, asset.Hash, from);
                    fromBalance.TransactionsCount++;
                    fromBalance.Balance -= ta.Amount;
                    if (fromBalance.Balance < 0)
                    {
                        fromBalance.Balance = -1;
                    }

                    var toBalance = this.GetBalance(db, asset.Hash, to);
                    toBalance.TransactionsCount++;
                    toBalance.Balance += ta.Amount;
                }
                else
                {
                    Log.Information($@"Notification of type - {type} has been thrown by contract - {item.ScriptHash}
                        This is for tx = {transaction.Hash.ToString()}");
                }

                this.state.Contracts.SetOrAddNotificationsForContract(item.ScriptHash.ToString(), item.ScriptHash.ToString(), blockTime.ToUnixTimestamp(), type, notificationStringArray);
                this.notificationHub
                    .Clients
                    .Group(item.ScriptHash.ToString())
                    .SendAsync("contract", this.state.Contracts.GetNotificationsFor(item.ScriptHash.ToString()));
                this.notificationHub
                    .Clients
                    .All
                    .SendAsync("all", this.state.Contracts.GetNotificationsFor(NotificationConstants.AllNotificationsKey));
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

        private void SaveEmitAndClear(StateOfNeoContext db, Block block, int transactions)
        {
            var currentStats = Mapper.Map<HeaderStatsViewModel>(block);
            currentStats.TransactionCount = transactions;

            var detailedTransactionsList = block.Transactions
                .AsQueryable()
                .ProjectTo<TransactionDetailedListViewModel>()
                .ToList();

            this.state.AddToDetailedTransactionsList(detailedTransactionsList);

            var transactionList = detailedTransactionsList
                .AsQueryable()
                .ProjectTo<TransactionListViewModel>()
                .ToList();

            this.state.AddToTransactionsList(transactionList);

            this.state.MainStats.SetHeaderStats(currentStats);
            this.state.MainStats.AddToTotalTxCount(transactions);

            this.state.MainStats.AddTotalBlocksCount(1);
            this.state.MainStats.AddToTotalBlocksSizesCount(block.Size);
            this.state.MainStats.AddToTotalBlocksTimesCount((decimal)block.TimeInSeconds);

            db.TotalStats.Update(this.state.MainStats.TotalStats);
            db.SaveChanges();

            this.EmitStatsInfo();
            this.txHub.Clients.All.SendAsync("new", detailedTransactionsList);

            this.pendingAddresses.Clear();
            this.pendingAssets.Clear();
            this.pendingBalances.Clear();
            this.pendingSmartContracts.Clear();
            this.pendingConsensusNodes.Clear();
        }

        private void EmitStatsInfo()
        {
            // Header
            this.statsHub.Clients.All.SendAsync("header", this.state.MainStats.GetHeaderStats());
            // Blocks
            this.statsHub.Clients.All.SendAsync("total-block-count", this.state.MainStats.GetTotalBlocksCount());
            this.statsHub.Clients.All.SendAsync("total-block-time", this.state.MainStats.GetTotalBlocksTimesCount());
            this.statsHub.Clients.All.SendAsync("total-block-size", this.state.MainStats.GetTotalBlocksSizesCount());
            // Transactions
            this.statsHub.Clients.All.SendAsync("tx-count", this.state.MainStats.GetTotalTxCount());
            this.statsHub.Clients.All.SendAsync("total-claimed", this.state.MainStats.GetTotalClaimed());
            // Addresses
            this.statsHub.Clients.All.SendAsync("address-count", this.state.MainStats.GetTotalAddressCount());
            // Assets
            this.statsHub.Clients.All.SendAsync("assets-count", this.state.MainStats.GetTotalAssetsCount());
            this.statsHub.Clients.All.SendAsync("gas-neo-tx-count", this.state.MainStats.GetTotalGasAndNeoTxCount());
            this.statsHub.Clients.All.SendAsync("nep-5-tx-count", this.state.MainStats.GetTotalNep5TxCount());
        }

        private TimeSpan TimeSpanBetweenGenesisAndNow()
        {
            return (DateTime.UtcNow - this.genesisTime);
        }

        private Asset GetAsset(StateOfNeoContext db, string hash)
        {
            var asset = db.Assets.Where(x => x.Hash == hash).FirstOrDefault();
            if (asset == null)
            {
                asset = this.pendingAssets.Where(x => x.Hash == hash).FirstOrDefault();
            }

            return asset;
        }

        private AddressAssetBalance GetBalance(StateOfNeoContext db, string hash, string address)
        {
            var balance = db.AddressBalances
                .Include(x => x.Asset)
                .Where(x => x.Asset.Hash == hash && x.AddressPublicAddress == address)
                .FirstOrDefault();

            if (balance == null)
            {
                balance = pendingBalances.FirstOrDefault(x => x.AddressPublicAddress == address && x.Asset.Hash == hash);
            }

            if (balance == null)
            {
                balance = new AddressAssetBalance
                {
                    AddressPublicAddress = address,
                    AssetHash = hash,
                    CreatedOn = DateTime.UtcNow,
                    Balance = 0
                };

                db.AddressBalances.Add(balance);
                pendingBalances.Add(balance);
            }

            return balance;
        }

        private ConsensusNode GetConsensusNode(StateOfNeoContext db, string address)
        {
            //foreach (var item in Blockchain.Singleton.GetSnapshot().Contracts)
            //{

            //}
            var consensusNode = db.ConsensusNodes.FirstOrDefault(x => x.Address == address);

            if (consensusNode == null)
            {
                consensusNode = new ConsensusNode
                {
                    Address = address
                };

                db.ConsensusNodes.Add(consensusNode);
                this.pendingConsensusNodes.Add(consensusNode);
            }

            return consensusNode;
        }

        private Data.Models.Address GetAddress(StateOfNeoContext db, string address, DateTime blockTime)
        {
            var result = db.Addresses
                .Include(x => x.Balances)
                .ThenInclude(x => x.Asset)
                .Where(x => x.PublicAddress == address)
                .FirstOrDefault();

            if (result == null)
            {
                result = pendingAddresses.FirstOrDefault(x => x.PublicAddress == address);
            }

            if (result == null)
            {
                result = new Data.Models.Address
                {
                    PublicAddress = address,
                    CreatedOn = DateTime.UtcNow,
                    FirstTransactionOn = blockTime,
                    LastTransactionOn = blockTime,
                    LastTransactionStamp = blockTime.ToUnixTimestamp(),
                };

                db.Addresses.Add(result);
                pendingAddresses.Add(result);

                this.state.MainStats.AddTotalAddressCount(1);
                this.state.AddAddresses(1, blockTime);
            }

            return result;
        }

        private void SeedGenesisBlock(StateOfNeoContext db)
        {
            var genesisBlock = new Block
            {
                Hash = this.hashesByNet["genesisBlock-" + this.net],
                Height = 0,
                Timestamp = GenesisBlock.Timestamp,
                Size = GenesisBlock.Size,
                ConsensusData = GenesisBlock.ConsensusData,
                NextConsensusNodeAddress = GenesisBlock.NextConsensus.ToAddress()
            };

            var minerTransaction = new Transaction
            {
                Type = Neo.Network.P2P.Payloads.TransactionType.MinerTransaction,
                CreatedOn = DateTime.UtcNow,
                Hash = "0xfb5bd72b2d6792d75dc2f1084ffa9e9f70ca85543c717a6b13d9959b452a57d6",
                Size = 10,
                MinerTransaction = new MinerTransaction
                {
                    CreatedOn = DateTime.UtcNow,
                    Nonce = 2083236893
                },
                Timestamp = genesisBlock.Timestamp
            };

            var neoRegisterTransaction = new Transaction
            {
                Type = Neo.Network.P2P.Payloads.TransactionType.RegisterTransaction,
                Hash = AssetConstants.NeoAssetId,
                Size = 107,
                CreatedOn = DateTime.UtcNow,
                RegisterTransaction = new RegisterTransaction
                {
                    CreatedOn = DateTime.UtcNow,
                    AssetType = Neo.Network.P2P.Payloads.AssetType.GoverningToken,
                    Name = GoverningToken.Name,
                    Amount = (decimal)GoverningToken.Amount,
                    OwnerPublicKey = GoverningToken.Owner.ToString(),
                    AdminAddress = GoverningToken.Admin.ToAddress(),
                    Precision = GoverningToken.Precision
                },
                Timestamp = genesisBlock.Timestamp
            };

            var gasRegisterTransaction = new Transaction
            {
                Type = Neo.Network.P2P.Payloads.TransactionType.RegisterTransaction,
                Hash = AssetConstants.GasAssetId,
                Size = 106,
                CreatedOn = DateTime.UtcNow,
                RegisterTransaction = new RegisterTransaction
                {
                    CreatedOn = DateTime.UtcNow,
                    AdminAddress = UtilityToken.Admin.ToAddress(),
                    Amount = (decimal)UtilityToken.Amount,
                    AssetType = Neo.Network.P2P.Payloads.AssetType.UtilityToken,
                    Name = UtilityToken.Name,
                    OwnerPublicKey = UtilityToken.Owner.ToString(),
                    Precision = UtilityToken.Precision
                },
                Timestamp = genesisBlock.Timestamp
            };

            var neo = new Asset
            {
                Hash = neoRegisterTransaction.Hash,
                CreatedOn = DateTime.UtcNow,
                Name = "NEO",
                Symbol = "NEO",
                MaxSupply = 100_000_000,
                Type = AssetType.NEO,
                GlobalType = Neo.Network.P2P.Payloads.AssetType.GoverningToken,
                CurrentSupply = 100_000_000,
                Decimals = 0,
                TransactionsCount = 1
            };

            var gas = new Asset
            {
                Hash = gasRegisterTransaction.Hash,
                CreatedOn = DateTime.UtcNow,
                Name = "GAS",
                Symbol = "GAS",
                MaxSupply = 100_000_000,
                Type = AssetType.GAS,
                GlobalType = Neo.Network.P2P.Payloads.AssetType.UtilityToken,
                Decimals = 8
            };

            var neoAssetIssueTransaction = new Transaction
            {
                Type = Neo.Network.P2P.Payloads.TransactionType.IssueTransaction,
                Hash = this.hashesByNet["neoIssue-" + this.net],
                Size = 69,
                Timestamp = genesisBlock.Timestamp
            };

            var assetInTransaction = new AssetInTransaction
            {
                Asset = neo,
                CreatedOn = DateTime.UtcNow,
                TransactionHash = neoAssetIssueTransaction.Hash,
                Timestamp = genesisBlock.Timestamp
            };

            neoAssetIssueTransaction.AssetsInTransactions.Add(assetInTransaction);

            var toAddress = new Data.Models.Address
            {
                PublicAddress = Contract.CreateMultiSigRedeemScript(StandbyValidators.Length / 2 + 1, StandbyValidators)
                    .ToScriptHash()
                    .ToAddress(),
                FirstTransactionOn = GenesisBlock.Timestamp.ToUnixDate(),
                LastTransactionOn = GenesisBlock.Timestamp.ToUnixDate(),
                LastTransactionStamp = GenesisBlock.Timestamp,
                TransactionsCount = 1
            };

            var addressInAssetTransaction = new AddressInAssetTransaction
            {
                AddressPublicAddress = toAddress.PublicAddress,
                Amount = 100000000,
                AssetInTransaction = assetInTransaction
            };

            assetInTransaction.AddressesInAssetTransactions.Add(addressInAssetTransaction);

            var transactedAsset = new TransactedAsset
            {
                Amount = 100000000,
                AssetType = AssetType.NEO,
                ToAddress = toAddress,
                Asset = neo,
                OutGlobalTransactionHash = neoAssetIssueTransaction.Hash,
                CreatedOn = DateTime.UtcNow
            };

            neoAssetIssueTransaction.GlobalOutgoingAssets.Add(transactedAsset);

            var balance = new AddressAssetBalance
            {
                CreatedOn = DateTime.UtcNow,
                Address = toAddress,
                Asset = neo,
                Balance = transactedAsset.Amount,
                TransactionsCount = 1
            };

            var addressInTransaction = new AddressInTransaction
            {
                AddressPublicAddress = toAddress.PublicAddress,
                Amount = balance.Balance,
                AssetHash = neo.Hash,
                CreatedOn = DateTime.UtcNow,
                Timestamp = genesisBlock.Timestamp,
                TransactionHash = neoAssetIssueTransaction.Hash
            };

            neoAssetIssueTransaction.AddressesInTransactions.Add(addressInTransaction);

            db.AddressBalances.Add(balance);

            db.Assets.Add(neo);
            db.Assets.Add(gas);

            genesisBlock.Transactions.Add(minerTransaction);
            genesisBlock.Transactions.Add(neoRegisterTransaction);
            genesisBlock.Transactions.Add(gasRegisterTransaction);
            genesisBlock.Transactions.Add(neoAssetIssueTransaction);

            db.Blocks.Add(genesisBlock);

            db.SaveChanges();
        }
    }
}
