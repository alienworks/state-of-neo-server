using System;
using System.Collections.Generic;
using System.Linq;
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
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Enums;
using StateOfNeo.Data.Models.Transactions;
using StateOfNeo.Server.Actors.Notifications;
using StateOfNeo.Server.Hubs;
using StateOfNeo.ViewModels;
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
        private readonly IHubContext<BlockHub> blockHub;

        private readonly ICollection<Data.Models.Asset> pendingAssets = new List<Data.Models.Asset>();
        private readonly ICollection<Data.Models.Address> pendingAddresses = new List<Data.Models.Address>();
        private readonly ICollection<Data.Models.AddressAssetBalance> pendingBalances = new List<Data.Models.AddressAssetBalance>();

        public BlockPersister(IActorRef blockchain, string connectionString, IHubContext<BlockHub> blockHub, string net)
        {
            this.connectionString = connectionString;
            this.blockHub = blockHub;
            this.net = net;

            blockchain.Tell(new Register());
        }

        public static Props Props(IActorRef blockchain, string connectionString, IHubContext<BlockHub> blockHub, string net) =>
            Akka.Actor.Props.Create(() => new BlockPersister(blockchain, connectionString, blockHub, net));

        protected override void OnReceive(object message)
        {
            if (message is PersistCompleted m)
            {
                var optionsBuilder = new DbContextOptionsBuilder<StateOfNeoContext>();
                optionsBuilder.UseSqlServer(this.connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
                var db = new StateOfNeoContext(optionsBuilder.Options);

                if (db.Blocks.Any(x => x.Hash == m.Block.Hash.ToString()))
                {
                    return;
                }

                if (m.Block.Index == 1)
                {
                    this.SeedGenesisBlock(db);
                }

                var currentHeight = db.Blocks.OrderByDescending(x => x.Height).Select(x => x.Height).FirstOrDefault();

                Block persisted = null;
                Neo.Network.P2P.Payloads.Block block = null;

                while (currentHeight < m.Block.Index)
                {
                    var hash = Blockchain.Singleton.GetBlockHash((uint)currentHeight + 1);
                    block = Blockchain.Singleton.GetBlock(hash);
                    persisted = this.PersistBlock(block, db);
                    currentHeight++;
                    if (currentHeight % 100 == 0)
                    {
                        this.SaveEmitAndClear(db, persisted, block.Transactions.Length);
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
                CreatedOn = blockToPersist.Timestamp.ToUnixDate(),
                ConsensusData = blockToPersist.ConsensusData,
                InvocationScript = blockToPersist.Witness.InvocationScript.ToHexString(),
                VerificationScript = blockToPersist.Witness.VerificationScript.ToHexString(),
                NextConsensusNodeAddress = blockToPersist.NextConsensus.ToString(),
                PreviousBlockHash = blockToPersist.PrevHash.ToString()
            };

            double timeInSeconds = 20;
            if (block.Height > 0)
            {
                var hash = Blockchain.Singleton.GetBlockHash((uint)block.Height - 1);
                var previousBlock = Blockchain.Singleton.GetBlock(hash);
                var previousBlockTime = previousBlock.Timestamp.ToUnixDate();

                timeInSeconds = (block.CreatedOn - previousBlockTime).TotalSeconds;
            }

            block.TimeInSeconds = timeInSeconds;

            db.Blocks.Add(block);

            foreach (var item in blockToPersist.Transactions)
            {
                var transaction = new Transaction
                {
                    Type = item.Type,
                    ScriptHash = item.Hash.ToString(),
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

                    if (unboxed.Name.Length > 2)
                    {
                        var test = JObject.Parse(unboxed.Name.Substring(1, unboxed.Name.Length - 2));
                        var nameTest = test["name"]?.AsString();
                    }

                    var asset = new Asset
                    {
                        CreatedOn = DateTime.UtcNow,
                        Name = unboxed.Name,
                        MaxSupply = (long)unboxed.Amount,
                        GlobalType = unboxed.AssetType,
                        Type = AssetType.OTHER,
                        Hash = item.Hash.ToString(),
                        Decimals = unboxed.Precision,
                        Symbol = unboxed.Name
                    };

                    db.Assets.Add(asset);
                    pendingAssets.Add(asset);
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

                    this.TrackInvocationTransaction(unboxed, db);

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

                for (int i = 0; i < item.Inputs.Length; i++)
                {
                    var input = item.Inputs[i];
                    var blockTime = block.Timestamp.ToUnixDate();
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
                        AssetId = asset.Id,
                        FromAddressPublicAddress = fromPublicAddress                        
                    };

                    fromAddress.LastTransactionOn = blockTime;
                    var fromBalance = this.GetBalance(db, asset.Hash, fromAddress.PublicAddress, asset.Id);
                    fromBalance.Balance -= ta.Amount;

                    transaction.GlobalIncomingAssets.Add(ta);
                }
                
                for (int i = 0; i < item.Outputs.Length; i++)
                {
                    var output = item.Outputs[i];
                    var blockTime = block.Timestamp.ToUnixDate();
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
                        AssetId = asset.Id,
                        ToAddressPublicAddress = toPublicAddress
                    };

                    toAddress.LastTransactionOn = blockTime;
                    var toBalance = this.GetBalance(db, asset.Hash, toAddress.PublicAddress, asset.Id);
                    toBalance.Balance += ta.Amount;

                    transaction.GlobalOutgoingAssets.Add(ta);
                }

                block.Transactions.Add(transaction);
            }

            return block;
        }

        private void TrackInvocationTransaction(Neo.Network.P2P.Payloads.InvocationTransaction transaction, StateOfNeoContext db)
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
                            Type = Data.Models.Enums.AssetType.NEP5,
                            Decimals = decimals,
                            CurrentSupply = totalSupply,
                            Symbol = symbol
                        };

                        db.Assets.Add(asset);
                        this.pendingAssets.Add(asset);
                    }

                    var notification = item.GetNotification<TransferNotification>();
                    var from = new UInt160(notification.From).ToAddress();
                    var to = new UInt160(notification.To).ToAddress();
                    var fromAddress = this.GetAddress(db, from, DateTime.UtcNow);

                    var toAddress = this.GetAddress(db, to, DateTime.UtcNow);
                    var ta = new Data.Models.Transactions.TransactedAsset
                    {
                        Amount = (decimal)notification.Amount,
                        Asset = asset,
                        FromAddressPublicAddress = from,
                        ToAddressPublicAddress = to,
                        AssetType = Data.Models.Enums.AssetType.NEP5,
                        CreatedOn = DateTime.UtcNow,
                        TransactionScriptHash = transaction.Hash.ToString()
                    };

                    
                    db.TransactedAssets.Add(ta);

                    var fromBalance = this.GetBalance(db, asset.Hash, from, asset.Id);
                    fromBalance.Balance -= ta.Amount;
                    if (fromBalance.Balance < 0)
                    {
                        fromBalance.Balance = -1;
                    }

                    var toBalance = this.GetBalance(db, asset.Hash, to, asset.Id);
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
            this.pendingAddresses.Clear();
            this.pendingBalances.Clear();
            this.pendingAssets.Clear();

            var hubBlock = Mapper.Map<BlockHubViewModel>(block);
            hubBlock.TransactionCount = transactions;
            this.blockHub.Clients.All.SendAsync("Receive", hubBlock);
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

        private AddressAssetBalance GetBalance(StateOfNeoContext db, string hash, string address, int assetId)
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
                    AssetId = assetId,
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
                ScriptHash = "0xfb5bd72b2d6792d75dc2f1084ffa9e9f70ca85543c717a6b13d9959b452a57d6",
                Size = 10,
                MinerTransaction = new MinerTransaction
                {
                    CreatedOn = DateTime.UtcNow,
                    Nonce = 2083236893
                }
            };

            var neoRegisterTransaction = new Transaction
            {
                Type = Neo.Network.P2P.Payloads.TransactionType.RegisterTransaction,
                ScriptHash = AssetConstants.NeoAssetId,
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
                }
            };

            var gasRegisterTransaction = new Transaction
            {
                Type = Neo.Network.P2P.Payloads.TransactionType.RegisterTransaction,
                ScriptHash = AssetConstants.GasAssetId,
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
                }
            };

            var neo = new Asset
            {
                Hash = neoRegisterTransaction.ScriptHash,
                CreatedOn = DateTime.UtcNow,
                Name = "NEO",
                Symbol = "NEO",
                MaxSupply = 100_000_000,
                Type = AssetType.NEO,
                GlobalType = Neo.Network.P2P.Payloads.AssetType.GoverningToken,
                CurrentSupply = 100_000_000,
                Decimals = 0
            };

            var gas = new Asset
            {
                Hash = gasRegisterTransaction.ScriptHash,
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
                ScriptHash = this.hashesByNet["neoIssue-" + this.net],
                Size = 69
            };

            var toAddress = new Data.Models.Address
            {
                PublicAddress = Contract.CreateMultiSigRedeemScript(StandbyValidators.Length / 2 + 1, StandbyValidators)
                    .ToScriptHash()
                    .ToAddress(),
                FirstTransactionOn = GenesisBlock.Timestamp.ToUnixDate(),
                LastTransactionOn = GenesisBlock.Timestamp.ToUnixDate()
            };

            var transactedAsset = new TransactedAsset
            {
                Amount = 100000000,
                AssetType = AssetType.NEO,
                ToAddress = toAddress,
                Asset = neo,
                OutGlobalTransactionScriptHash = neoAssetIssueTransaction.ScriptHash,
                CreatedOn = DateTime.UtcNow
            };

            neoAssetIssueTransaction.GlobalOutgoingAssets.Add(transactedAsset);

            var balance = new AddressAssetBalance
            {
                CreatedOn = DateTime.UtcNow,
                Address = toAddress,
                Asset = neo,
                Balance = transactedAsset.Amount                
            };

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
