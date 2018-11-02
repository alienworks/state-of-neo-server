using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Neo;
using Neo.Ledger;
using Neo.SmartContract;
using Neo.Wallets;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Enums;
using StateOfNeo.Data.Models.Transactions;
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

        public BlockPersister(IActorRef blockchain, string connectionString,
            IHubContext<BlockHub> blockHub, string net)
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
                optionsBuilder.UseSqlServer(this.connectionString);
                var db = new StateOfNeoContext(optionsBuilder.Options);

                if (db.Blocks.Any(x => x.Hash == m.Block.Hash.ToString()))
                {
                    return;
                }

                if (m.Block.Index == 1)
                {
                    this.SeedGenesisBlock(db);
                }

                var currentHeight = db.Blocks
                    .OrderByDescending(x => x.Height)
                    .Select(x => x.Height)
                    .FirstOrDefault();

                while (currentHeight < m.Block.Index)
                {
                    var hash = Blockchain.Singleton.GetBlockHash((uint)currentHeight + 1);
                    var block = Blockchain.Singleton.GetBlock(hash);
                    this.PersistBlock(block, db);
                    currentHeight++;
                }
                
                db.Dispose();
            }
        }

        private void PersistBlock(Neo.Network.P2P.Payloads.Block blockToPersist, StateOfNeoContext db)
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

            var hubBlock = Mapper.Map<BlockHubViewModel>(block);
            hubBlock.TransactionCount = blockToPersist.Transactions.Length;
            this.blockHub.Clients.All.SendAsync("Receive", hubBlock);

            db.Blocks.Add(block);
            db.SaveChanges();

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
                    Version = item.Version
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

                for (int i = 0; i < item.Outputs.Length; i++)
                {
                    var output = item.Outputs[i];
                    Neo.Network.P2P.Payloads.CoinReference input = null;
                    if (item.Inputs.Any())
                    {
                        input = item.Inputs[0];
                    }
                    else if (item is Neo.Network.P2P.Payloads.ClaimTransaction claimTransaction)
                    {
                        input = claimTransaction.Claims[0];
                    }
                    else if (item is Neo.Network.P2P.Payloads.MinerTransaction minerTransaction)
                    {

                    }

                    Transaction previousTransaction = null;
                    if (input != null)
                    {
                        previousTransaction = db.Transactions
                           .Include(x => x.Assets)
                           .ThenInclude(x => x.ToAddress)
                           .Where(x => x.ScriptHash == input.PrevHash.ToString())
                           .FirstOrDefault();
                    }

                    Data.Models.Address fromAddress = null;
                    if (previousTransaction != null && item is Neo.Network.P2P.Payloads.ClaimTransaction == false)
                    {
                        fromAddress = previousTransaction
                            .Assets
                            .Skip(input.PrevIndex)
                            .Select(x => x.ToAddress)
                            .FirstOrDefault();
                    }
                    
                    var toAddress = db.Addresses
                        .Include(x => x.Balances).ThenInclude(x => x.Asset)
                        .Where(x => x.PublicAddress == output.ScriptHash.ToAddress())
                        .FirstOrDefault();

                    if (toAddress == null)
                    {
                        toAddress = new Data.Models.Address
                        {
                            PublicAddress = output.ScriptHash.ToAddress(),
                            CreatedOn = DateTime.UtcNow,
                            FirstTransactionOn = block.Timestamp.ToUnixDate()
                        };

                        db.Addresses.Add(toAddress);
                        db.SaveChanges();
                    }

                    toAddress.LastTransactionOn = block.Timestamp.ToUnixDate();

                    var asset = db.Assets.Where(x => x.Hash == output.AssetId.ToString()).FirstOrDefault();
                    if (asset == null)
                    {
                        asset = new Asset
                        {
                            CreatedOn = DateTime.UtcNow,
                            Hash = output.AssetId.ToString()
                        };

                        db.Assets.Add(asset);
                        db.SaveChanges();
                    }
                                        
                    var ta = new TransactedAsset
                    {
                        Amount = (decimal)output.Value,
                        FromAddress = fromAddress,
                        ToAddress = toAddress,
                        AssetType = output.AssetId.ToString() == "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b" ? AssetType.NEO : AssetType.GAS
                    };

                    var toBalance = db.AddressBalances
                        .Include(x => x.Asset)
                        .Where(x => x.Asset.Hash == asset.Hash && x.AddressPublicAddress == toAddress.PublicAddress)
                        .FirstOrDefault();

                    if (toBalance == null)
                    {
                        toBalance = new AddressAssetBalance
                        {
                            AddressPublicAddress = toAddress.PublicAddress,
                            AssetId = asset.Id,
                            CreatedOn = DateTime.UtcNow,
                            Balance = 0
                        };

                        db.AddressBalances.Add(toBalance);
                        db.SaveChanges();
                    }

                    if (fromAddress != null)
                    {
                        fromAddress.LastTransactionOn = block.Timestamp.ToUnixDate();

                        var fromBalance = db.AddressBalances
                            .Include(x => x.Asset)
                            .Where(x => x.Asset.Hash == asset.Hash && x.AddressPublicAddress == fromAddress.PublicAddress)
                            .FirstOrDefault();

                        if (fromBalance == null)
                        {
                            fromBalance = new AddressAssetBalance
                            {
                                AddressPublicAddress = fromAddress.PublicAddress,
                                AssetId = asset.Id,
                                CreatedOn = DateTime.UtcNow,
                                Balance = 0
                            };

                            db.AddressBalances.Add(fromBalance);
                            db.SaveChanges();
                        }

                        fromBalance.Balance -= ta.Amount;
                        db.SaveChanges();

                        if (transaction.SystemFee != 0)
                        {
                            var fromAddressGasBalance = db.AddressBalances
                                .Include(x => x.Asset)
                                .Where(x => x.Asset.Type == AssetType.GAS && x.AddressPublicAddress == fromAddress.PublicAddress)
                                .FirstOrDefault();

                            fromAddressGasBalance.Balance -= transaction.SystemFee;

                            db.SaveChanges();
                        }
                    }

                    toBalance.Balance += ta.Amount;

                    asset.TransactedAssets.Add(ta);
                    transaction.Assets.Add(ta);
                    db.SaveChanges();
                }

                block.Transactions.Add(transaction);
            }

            db.SaveChanges();
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
                ScriptHash = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b",
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
                ScriptHash = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7",
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
                MaxSupply = 100_000_000,
                Type = AssetType.NEO
            };

            var gas = new Asset
            {
                Hash = gasRegisterTransaction.ScriptHash,
                CreatedOn = DateTime.UtcNow,
                Name = "GAS",
                MaxSupply = 100_000_000,
                Type = AssetType.GAS
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
                FirstTransactionOn = GenesisBlock.Timestamp.ToUnixDate()
            };

            var transactedAsset = new TransactedAsset
            {
                Amount = 100000000,
                AssetType = AssetType.NEO,
                ToAddress = toAddress,
                Asset = neo
            };

            neoAssetIssueTransaction.Assets.Add(transactedAsset);

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
