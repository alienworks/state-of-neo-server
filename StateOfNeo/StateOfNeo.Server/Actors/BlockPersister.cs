using System;
using System.Linq;
using Akka.Actor;
using Microsoft.EntityFrameworkCore;
using Neo;
using Neo.SmartContract;
using Neo.Wallets;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Transactions;
using static Neo.Ledger.Blockchain;

namespace StateOfNeo.Server.Actors
{
    public class BlockPersister : UntypedActor
    {
        private readonly string connectionString;
        
        public BlockPersister(IActorRef blockchain, string connectionString)
        {
            this.connectionString = connectionString;

            blockchain.Tell(new Register());
        }

        public static Props Props(IActorRef blockchain, string connectionString) =>
            Akka.Actor.Props.Create(() => new BlockPersister(blockchain, connectionString));

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

                var persistedBlock = m.Block;
                var createdOn = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                createdOn = createdOn.AddSeconds(persistedBlock.Timestamp).ToLocalTime();

                var block = new Block
                {
                    Hash = persistedBlock.Hash.ToString(),
                    Height = (int)persistedBlock.Header.Index,
                    Size = persistedBlock.Size,
                    Timestamp = persistedBlock.Timestamp,
                    Validator = persistedBlock.Witness.ScriptHash.ToString(),
                    CreatedOn = createdOn
                };

                db.Blocks.Add(block);
                db.SaveChanges();

                foreach (var item in persistedBlock.Transactions)
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
                        
                        var previousTransaction = db.Transactions
                            .Include(x => x.Assets)
                            .ThenInclude(x => x.ToAddress)
                            .Where(x => x.ScriptHash == input.PrevHash.ToString())
                            .FirstOrDefault();

                        Data.Models.Address fromAddress = null;
                        if (previousTransaction != null)
                        {
                            fromAddress = previousTransaction
                                .Assets
                                .Skip(input.PrevIndex)
                                .Select(x => x.ToAddress)
                                .FirstOrDefault();
                        }

                        var toAddress = db.Addresses
                            .Where(x => x.PublicAddress == output.ScriptHash.ToAddress())
                            .FirstOrDefault();

                        if (toAddress == null)
                        {
                            toAddress = new Data.Models.Address
                            {
                                PublicAddress = output.ScriptHash.ToAddress(),
                                CreatedOn = DateTime.UtcNow
                            };

                            db.Addresses.Add(toAddress);
                        }

                        var ta = new TransactedAsset
                        {
                            Amount = (decimal)output.Value,
                            FromAddress = fromAddress,
                            ToAddress = toAddress,
                        };

                        transaction.Assets.Add(ta);
                        db.SaveChanges();
                    }

                    block.Transactions.Add(transaction);
                }

                db.SaveChanges();
                db.Dispose();
            }
        }

        private void SeedGenesisBlock(StateOfNeoContext db)
        {
            var genesisBlock = new Block
            {
                Hash = "0x996e37358dc369912041f966f8c5d8d3a8255ba5dcbd3447f8a82b55db869099",
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

            var neoAssetIssueTransaction = new Transaction
            {
                Type = Neo.Network.P2P.Payloads.TransactionType.IssueTransaction,
                ScriptHash = "0x7aadf91ca8ac1e2c323c025a7e492bee2dd90c783b86ebfc3b18db66b530a76d",
                Size = 69                
            };

            neoAssetIssueTransaction.Assets.Add(new TransactedAsset
            {
                Amount = 100000000,
                AssetType = Data.Models.Enums.GlobalAssetType.Neo,
                ToAddress = new Data.Models.Address
                {
                    PublicAddress = Contract.CreateMultiSigRedeemScript(StandbyValidators.Length / 2 + 1, StandbyValidators)
                        .ToScriptHash()
                        .ToAddress()
                }
            });

            genesisBlock.Transactions.Add(minerTransaction);
            genesisBlock.Transactions.Add(neoRegisterTransaction);
            genesisBlock.Transactions.Add(gasRegisterTransaction);
            genesisBlock.Transactions.Add(neoAssetIssueTransaction);

            db.Blocks.Add(genesisBlock);

            db.SaveChanges();
        }
    }
}
