using Akka.Actor;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Neo;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using StateOfNeo.Common;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Transactions;
using StateOfNeo.Server.Actors.Notifications;
using StateOfNeo.Server.Hubs;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private readonly ICollection<Data.Models.Asset> pendingAssets = new List<Data.Models.Asset>();
        private readonly ICollection<Data.Models.Address> pendingAddresses = new List<Data.Models.Address>();
        private readonly ICollection<Data.Models.AddressAssetBalance> pendingBalances = new List<Data.Models.AddressAssetBalance>();

        private long totalTxCount = 0;
        private int totalAddressCount = 0;
        private int totalAssetsCount = 0;

        public BlockPersister(IActorRef blockchain, string connectionString, IHubContext<StatsHub> statsHub, string net)
        {
            this.connectionString = connectionString;
            this.statsHub = statsHub;
            this.net = net;

            blockchain.Tell(new Register());
        }

        public static Props Props(IActorRef blockchain, string connectionString, IHubContext<StatsHub> statsHub, string net) =>
            Akka.Actor.Props.Create(() => new BlockPersister(blockchain, connectionString, statsHub, net));

        protected override void OnReceive(object message)
        {
            if (message is PersistCompleted m)
            {
                var db = StateOfNeoContext.Create(this.connectionString);                
                if (db.Blocks.Any(x => x.Hash == m.Block.Hash.ToString()))
                {
                    return;
                }

                if (!db.Blocks.Any(x => x.Hash == this.hashesByNet["genesisBlock-" + this.net]))
                {
                    this.SeedGenesisBlock(db);
                }

                if (this.totalTxCount == 0)
                {
                    this.totalTxCount += db.Transactions.Count();
                }

                if (this.totalAddressCount == 0)
                {
                    this.totalAddressCount += db.Addresses.Count();
                }

                if (this.totalAssetsCount == 0)
                {
                    this.totalAssetsCount += db.Assets.Count();
                }

                var currentHeight = db.Blocks.OrderByDescending(x => x.Height).Select(x => x.Height).FirstOrDefault();

                Block persisted = null;
                Neo.Network.P2P.Payloads.Block block = null;
                while (currentHeight < m.Block.Index)
                {
                    var sw1 = System.Diagnostics.Stopwatch.StartNew();
                    var hash = Blockchain.Singleton.GetBlockHash((uint)currentHeight + 1);
                    block = Blockchain.Singleton.GetBlock(hash);
                    persisted = this.PersistBlock(block, db);
                    currentHeight++;
                    if (db.ChangeTracker.Entries().Count() > 10_000)
                    {
                        sw1.Stop();
                        var sw2 = System.Diagnostics.Stopwatch.StartNew();
                        this.SaveEmitAndClear(db, persisted, block.Transactions.Length);
                        sw2.Stop();

                        var totalTime = sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds;

                        db = StateOfNeoContext.Create(this.connectionString);
                    }
                }

                this.SaveEmitAndClear(db, persisted, block.Transactions.Length);

                db.Dispose();
            }
        }

        private Block PersistBlock(Neo.Network.P2P.Payloads.Block blockToPersist, StateOfNeoContext db)
        {
            var block = new Block
            {
                Hash = blockToPersist.Hash.ToString(),
                Height = (int)blockToPersist.Header.Index,
                Size = blockToPersist.Size,
                Timestamp = blockToPersist.Timestamp,
                Validator = blockToPersist.Witness.ScriptHash.ToString(),
                CreatedOn = DateTime.UtcNow,
                ConsensusData = blockToPersist.ConsensusData,
                InvocationScript = blockToPersist.Witness.InvocationScript.ToHexString(),
                VerificationScript = blockToPersist.Witness.VerificationScript.ToHexString(),
                NextConsensusNodeAddress = blockToPersist.NextConsensus.ToString(),
                PreviousBlockHash = blockToPersist.PrevHash.ToString(),
                TimeInSeconds = 20
            };

            var blockTime = block.Timestamp.ToUnixDate();

            if (block.Height > 0)
            {
                var hash = Blockchain.Singleton.GetBlockHash((uint)block.Height - 1);
                var previousBlock = Blockchain.Singleton.GetBlock(hash);
                var previousBlockTime = previousBlock.Timestamp.ToUnixDate();

                block.TimeInSeconds = (blockTime - previousBlockTime).TotalSeconds;
            }

            db.Blocks.Add(block);

            foreach (var item in blockToPersist.Transactions)
            {
                var transaction = new Transaction
                {
                    Type = item.Type,
                    Hash = item.Hash.ToString(),
                    CreatedOn = DateTime.UtcNow,
                    NetworkFee = (decimal)item.NetworkFee,
                    SystemFee = (decimal)item.SystemFee,
                    Size = item.Size,
                    Version = item.Version,
                    Timestamp = block.Timestamp
                };

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
                    totalAssetsCount++;
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
                    var fromBalance = this.GetBalance(db, asset.Hash, fromAddress.PublicAddress);
                    fromBalance.Balance -= ta.Amount;
                    this.AdjustTransactedAmount(transactedAmounts, assetHash, fromPublicAddress, -ta.Amount);

                    transaction.GlobalIncomingAssets.Add(ta);
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
                    var toBalance = this.GetBalance(db, asset.Hash, toAddress.PublicAddress);
                    toBalance.Balance += ta.Amount;
                    this.AdjustTransactedAmount(transactedAmounts, assetHash, toPublicAddress, ta.Amount);

                    transaction.GlobalOutgoingAssets.Add(ta);
                }

                foreach (var assetTransactions in transactedAmounts)
                {
                    var asset = this.GetAsset(db, assetTransactions.Key);
                    asset.TransactionsCount++;

                    var assetInTransaction = new AssetInTransaction
                    {
                        AssetHash = assetTransactions.Key,
                        TransactionHash = transaction.Hash,
                        CreatedOn = DateTime.UtcNow
                    };

                    transaction.AssetsInTransactions.Add(assetInTransaction);

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
            }

            return block;
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
            }

            foreach (var item in result.Notifications)
            {
                var type = item.GetNotificationType();
                if (type == "transfer")
                {
                    var name = this.TestInvoke(db, item.ScriptHash, "name").HexStringToString();
                    var assetHash = item.ScriptHash.ToString();
                    var asset = this.GetAsset(db, assetHash);
                    if (asset == null)
                    {
                        var symbol = this.TestInvoke(db, item.ScriptHash, "symbol").HexStringToString();

                        var decimalsHex = this.TestInvoke(db, item.ScriptHash, "decimals");
                        var decimals = Convert.ToInt32(decimalsHex, 16);

                        var totalSupplyHex = this.TestInvoke(db, item.ScriptHash, "totalSupply");
                        var totalSupply = Convert.ToInt64(totalSupplyHex, 16);

                        asset = new Asset
                        {
                            CreatedOn = DateTime.UtcNow,
                            GlobalType = null,
                            Hash = assetHash,
                            Name = name,
                            MaxSupply = totalSupply,
                            Type = AssetType.NEP5,
                            Decimals = decimals,
                            CurrentSupply = totalSupply,
                            Symbol = symbol
                        };

                        db.Assets.Add(asset);
                        this.pendingAssets.Add(asset);
                        this.totalAssetsCount++;
                    }

                    var assetInTransaction = new AssetInTransaction
                    {
                        AssetHash = asset.Hash,
                        CreatedOn = DateTime.UtcNow,
                        TransactionHash = transaction.Hash.ToString()
                    };

                    db.AssetsInTransactions.Add(assetInTransaction);

                    var notification = item.GetNotification<TransferNotification>();
                    var from = new UInt160(notification.From).ToAddress();
                    var to = new UInt160(notification.To).ToAddress();
                    var fromAddress = this.GetAddress(db, from, blockTime);
                    fromAddress.TransactionsCount++;

                    var toAddress = this.GetAddress(db, to, blockTime);
                    toAddress.TransactionsCount++;

                    var ta = new Data.Models.Transactions.TransactedAsset
                    {
                        Amount = (decimal)notification.Amount,
                        Asset = asset,
                        FromAddressPublicAddress = from,
                        ToAddressPublicAddress = to,
                        AssetType = AssetType.NEP5,
                        CreatedOn = DateTime.UtcNow,
                        TransactionHash = transaction.Hash.ToString()
                    };

                    db.TransactedAssets.Add(ta);

                    var fromAddressInTransaction = new AddressInTransaction
                    {
                        AddressPublicAddress = fromAddress.PublicAddress,
                        Amount = ta.Amount,
                        AssetHash = asset.Hash,
                        CreatedOn = DateTime.UtcNow,
                        Timestamp = blockTime.ToUnixTimestamp(),
                        TransactionHash = ta.TransactionHash
                    };

                    var toAddressInTransaction = new AddressInTransaction
                    {
                        AddressPublicAddress = toAddress.PublicAddress,
                        Amount = ta.Amount,
                        AssetHash = asset.Hash,
                        CreatedOn = DateTime.UtcNow,
                        Timestamp = blockTime.ToUnixTimestamp(),
                        TransactionHash = ta.TransactionHash
                    };

                    var fromAddressInAssetTransaction = new AddressInAssetTransaction
                    {
                        AddressPublicAddress = fromAddress.PublicAddress,
                        CreatedOn = DateTime.UtcNow,
                        Amount = ta.Amount
                    };

                    var toAddressInAssetTransaction = new AddressInAssetTransaction
                    {
                        AddressPublicAddress = toAddress.PublicAddress,
                        CreatedOn = DateTime.UtcNow,
                        Amount = ta.Amount
                    };

                    assetInTransaction.AddressesInAssetTransactions.Add(fromAddressInAssetTransaction);
                    assetInTransaction.AddressesInAssetTransactions.Add(toAddressInAssetTransaction);

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
            }
        }

        private string TestInvoke(StateOfNeoContext db, UInt160 contractHash, string operation, params object[] args)
        {
            using (var sb = new ScriptBuilder())
            {
                var parameters = new ContractParameter[]
                {
                    new ContractParameter { Type = ContractParameterType.String, Value = operation },
                    new ContractParameter { Type = ContractParameterType.Array, Value = new ContractParameter[0] }
                };

                sb.EmitAppCall(contractHash, parameters);
                var script = sb.ToArray();
                var engine = ApplicationEngine.Run(script, testMode: true);
                var result = engine.ResultStack.FirstOrDefault();
                if (result == null)
                {
                    return "";
                }

                return result.GetByteArray().ToHexString();
            }
        }

        private void SaveEmitAndClear(StateOfNeoContext db, Block block, int transactions)
        {
            db.SaveChanges();

            var hubBlock = Mapper.Map<BlockHubViewModel>(block);
            hubBlock.TransactionCount = transactions;
            this.statsHub.Clients.All.SendAsync("header", hubBlock);

            this.totalTxCount += transactions;
            this.statsHub.Clients.All.SendAsync("tx-count", this.totalTxCount);
            
            this.statsHub.Clients.All.SendAsync("address-count", this.totalAddressCount);
            this.statsHub.Clients.All.SendAsync("assets-count", this.totalAssetsCount);

            this.pendingAddresses.Clear();
            this.pendingBalances.Clear();
            this.pendingAssets.Clear();
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
                    LastTransactionOn = blockTime
                };

                db.Addresses.Add(result);
                pendingAddresses.Add(result);

                this.totalAddressCount++;
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
                TransactionHash = neoAssetIssueTransaction.Hash
            };

            neoAssetIssueTransaction.AssetsInTransactions.Add(assetInTransaction);

            var toAddress = new Data.Models.Address
            {
                PublicAddress = Contract.CreateMultiSigRedeemScript(StandbyValidators.Length / 2 + 1, StandbyValidators)
                    .ToScriptHash()
                    .ToAddress(),
                FirstTransactionOn = GenesisBlock.Timestamp.ToUnixDate(),
                LastTransactionOn = GenesisBlock.Timestamp.ToUnixDate(),
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
