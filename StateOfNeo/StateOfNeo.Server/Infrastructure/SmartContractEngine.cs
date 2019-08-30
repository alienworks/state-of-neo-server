using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Ledger;
using Neo.SmartContract;
using Neo.VM;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Actors.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class SmartContractEngine
    {
        private readonly string connectionString;
        private StateOfNeoContext db;
        private readonly ICollection<SmartContract> contracts;

        public SmartContractEngine(IOptions<DbSettings> dbSettings)
        {
            this.connectionString = dbSettings.Value.DefaultConnection;
            this.db = StateOfNeoContext.Create(this.connectionString);
            this.EnsureAllContractsFromLastSnapshot();

            this.contracts = this.db.SmartContracts.ToList();
        }

        public async Task Run()
        {
            var sw = Stopwatch.StartNew();

            Log.Information($"SmartContractEngine Run method started at {DateTime.UtcNow}");
            var dbContracts = this.db.SmartContracts.ToList();

            var blocksWithTransactions = this.db.Transactions
                .Include(x => x.InvocationTransaction)
                .Where(x => x.Type == Neo.Network.P2P.Payloads.TransactionType.InvocationTransaction)
                .Where(x => x.InvocationTransaction.SmartContractId == null || x.InvocationTransaction.TransactionHash == null)
                .Select(x => new { x.BlockId, x.Hash, x.InvocationTransactionId })
                .GroupBy(x => x.BlockId)
                .ToList();

            var i = 1;
            var swCounter = Stopwatch.StartNew();
            foreach (var blockWithTransactions in blocksWithTransactions)
            {
                var block = Blockchain.Singleton.GetBlock(UInt256.Parse(blockWithTransactions.Key));

                foreach (var dbTransaction in blockWithTransactions.ToList())
                {
                    if (i % 100_000 == 0)
                    {
                        this.db.SaveChanges();
                        Log.Information($"For 100_000 transactions time it took was {swCounter.ElapsedMilliseconds}");
                        i = 1;
                        swCounter.Restart();
                    }

                    var dbInvocationTransaction = this.db.InvocationTransactions
                        .FirstOrDefault(x => x.Id == dbTransaction.InvocationTransactionId);

                    if (dbInvocationTransaction != null)
                    {
                        if (dbInvocationTransaction.SmartContractId != null)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        dbInvocationTransaction = new Data.Models.Transactions.InvocationTransaction();
                    }

                    var blockTx = block.Transactions.FirstOrDefault(x => x.Hash.ToString() == dbTransaction.Hash);

                    if (blockTx != null)
                    {
                        var unboxed = blockTx as Neo.Network.P2P.Payloads.InvocationTransaction;

                        var appResult = this.GetExecutionResult(unboxed);
                        var scHash = appResult;

                        if (appResult == null)
                        {
                            continue;
                        }

                        var sc = this.contracts.FirstOrDefault(x => x.Hash == scHash.ToString());

                        if (sc != null)
                        {
                            if (sc.Timestamp == 0 || sc.Timestamp > block.Timestamp)
                            {
                                sc.Timestamp = block.Timestamp;
                            }

                            dbInvocationTransaction.ContractHash = sc.Hash;
                            dbInvocationTransaction.SmartContractId = sc.Id;
                            dbInvocationTransaction.TransactionHash = dbTransaction.Hash;
                        }

                        i++;
                    }
                }

                if (this.db.ChangeTracker.Entries().Count() > 10_000)
                {
                    Log.Information($"{nameof(this.db.ChangeTracker)} entries > 10_000 next is save changes");

                    await this.db.SaveChangesAsync();

                    Log.Information($"Save changes done!");
                    sw.Stop();
                    Log.Information($"Took {sw.ElapsedMilliseconds} ms for this iteration");

                    sw.Reset();
                    sw.Start();

                    this.db.Dispose();

                    this.db = StateOfNeoContext.Create(this.connectionString);
                }
            }

            Log.Information($"SmartContractEngine Run method ENDED at {DateTime.UtcNow}");
        }

        public UInt160 GetExecutionResult(Neo.Network.P2P.Payloads.InvocationTransaction transaction)
        {
            UInt160 contractHash = null;
            using (ApplicationEngine engine = new ApplicationEngine(
                TriggerType.Application,
                transaction,
                Blockchain.Singleton.GetSnapshot().Clone(),
                transaction.Gas,
                true))
            {
                engine.LoadScript(transaction.Script);
                while (
                    !engine.State.HasFlag(VMState.FAULT)
                    && engine.InvocationStack.Any()
                    && engine.CurrentContext.InstructionPointer != engine.CurrentContext.Script.Length)
                {
                    var nextOpCode = engine.CurrentContext.NextInstruction.OpCode;
                    if (nextOpCode == OpCode.APPCALL || nextOpCode == OpCode.TAILCALL)
                    {
                        contractHash = new UInt160(engine.CurrentContext.NextInstruction.Operand);
                        break;
                    }

                    var executeNextMethod = typeof(ExecutionEngine).GetMethod("ExecuteNext", BindingFlags.NonPublic | BindingFlags.Instance);
                    executeNextMethod.Invoke(engine, new object[0]);
                }
            }

            return contractHash;
        }

        private void EnsureAllContractsFromLastSnapshot()
        {
            var snapshot = Blockchain.Singleton.GetSnapshot();
            var contracts = snapshot.Contracts;
            var contractsList = contracts.Find();

            foreach (var sc in contractsList)
            {
                if (this.db.SmartContracts.Any(x => x.Hash == sc.Value.ScriptHash.ToString()))
                {
                    continue;
                }

                if (sc.Value == null)
                {
                    Log.Information($"Tryed to create not existing contract with hash: {sc.Value.ScriptHash}. Timestamp: {1}");
                    continue;
                }

                var newSc = new SmartContract
                {
                    Author = sc.Value.Author,
                    CreatedOn = DateTime.UtcNow,
                    Description = sc.Value.Description,
                    Email = sc.Value.Email,
                    HasDynamicInvoke = sc.Value.HasDynamicInvoke,
                    Hash = sc.Value.ScriptHash.ToString(),
                    HasStorage = sc.Value.HasStorage,
                    InputParameters = string.Join(",", sc.Value.ParameterList.Select(x => x)),
                    Name = sc.Value.Name,
                    Payable = sc.Value.Payable,
                    ReturnType = sc.Value.ReturnType,
                    //find stamp !!
                    //Timestamp = sc.Value.timestamp,
                    Version = sc.Value.CodeVersion
                };

                db.SmartContracts.Add(newSc);
                db.SaveChanges();
            }

        }
    }
}
