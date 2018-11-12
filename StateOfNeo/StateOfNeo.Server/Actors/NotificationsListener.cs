using Akka.Actor;
using Microsoft.EntityFrameworkCore;
using Neo;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using StateOfNeo.Common;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Actors.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using static Neo.Ledger.Blockchain;

namespace StateOfNeo.Server.Actors
{
    public class NotificationsListener : UntypedActor
    {
        private readonly string connectionString;

        public NotificationsListener(IActorRef blockchain, string connectionString)
        {
            blockchain.Tell(new Register());

            this.connectionString = connectionString;
        }

        protected override void OnReceive(object message)
        {
            if (message is ApplicationExecuted m)
            {
                var transaction = m.Transaction as InvocationTransaction;
                var optionsBuilder = new DbContextOptionsBuilder<StateOfNeoContext>();
                optionsBuilder.UseSqlServer(this.connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
                var db = new StateOfNeoContext(optionsBuilder.Options);

                foreach (var result in m.ExecutionResults)
                {
                    foreach (var item in result.Notifications)
                    {
                        var type = item.GetNotificationType();
                        if (type == "transfer")
                        {
                            var name = this.TestInvoke(item.ScriptHash, "name").HexStringToString();
                            var asset = db.Assets.Where(x => x.Name == name).FirstOrDefault();
                            if (asset == null)
                            {
                                var symbol = this.TestInvoke(item.ScriptHash, "symbol").HexStringToString();

                                var decimalsHex = this.TestInvoke(item.ScriptHash, "decimals");
                                var decimals = Convert.ToInt32(decimalsHex, 16);

                                var totalSupplyHex = this.TestInvoke(item.ScriptHash, "totalSupply");
                                var totalSupply = Convert.ToInt64(totalSupplyHex, 16);

                                asset = new Asset
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    GlobalType = null,
                                    Hash = item.ScriptHash.ToString(),
                                    Name = name,
                                    MaxSupply = totalSupply,
                                    Type = StateOfNeo.Common.Enums.AssetType.NEP5,
                                    Decimals = decimals,
                                    CurrentSupply = totalSupply,
                                    Symbol = symbol
                                };

                                db.Assets.Add(asset);
                                db.SaveChanges();
                            }

                            var notification = item.GetNotification<TransferNotification>();
                            var from = new UInt160(notification.From).ToAddress();
                            var to = new UInt160(notification.To).ToAddress();

                            var fromAddress = db.Addresses.Where(x => x.PublicAddress == from).FirstOrDefault();
                            if (fromAddress == null)
                            {

                            }

                            var toAddress = db.Addresses.Where(x => x.PublicAddress == to).FirstOrDefault();
                            if (toAddress == null)
                            {
                                toAddress = new Data.Models.Address
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    FirstTransactionOn = DateTime.UtcNow,
                                    LastTransactionOn = DateTime.UtcNow,
                                    PublicAddress = to
                                };

                                db.Addresses.Add(toAddress);
                                db.SaveChanges();
                            }

                            var ta = new Data.Models.Transactions.TransactedAsset
                            {
                                Amount = (decimal)notification.Amount,
                                Asset = asset,
                                FromAddressPublicAddress = from,
                                ToAddressPublicAddress = to,
                                AssetType = StateOfNeo.Common.Enums.AssetType.NEP5,
                                CreatedOn = DateTime.UtcNow,
                                TransactionScriptHash = transaction.Hash.ToString()
                            };

                            db.TransactedAssets.Add(ta);

                            var fromBalance = db.AddressBalances
                                .Where(x => x.AssetId == asset.Id && x.AddressPublicAddress == from)
                                .FirstOrDefault();

                            if (fromBalance == null)
                            {

                            }

                            fromBalance.Balance -= ta.Amount;
                            if (fromBalance.Balance < 0)
                            {

                            }

                            var toBalance = db.AddressBalances
                                .Where(x => x.AssetId == asset.Id && x.AddressPublicAddress == to)
                                .FirstOrDefault();

                            if (toBalance == null)
                            {
                                toBalance = new AddressAssetBalance
                                {
                                    AddressPublicAddress = toAddress.PublicAddress,
                                    Asset = asset,
                                    Balance = 0,
                                    CreatedOn = DateTime.UtcNow
                                };

                                db.AddressBalances.Add(toBalance);
                            }

                            toBalance.Balance += ta.Amount;

                            db.SaveChanges();
                        }
                    }
                }

                db.Dispose();
            }
        }

        public static Props Props(IActorRef blockchain, string connectionString) =>
            Akka.Actor.Props.Create(() => new NotificationsListener(blockchain, connectionString));

        private string TestInvoke(UInt160 contractHash, string operation, params object[] args)
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
    }
}
