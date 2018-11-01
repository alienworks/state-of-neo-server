using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using StateOfNeo.Data;
using StateOfNeo.Data.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using Neo.Network.P2P.Payloads;
using StateOfNeo.Data.Models.Enums;
using AutoMapper.QueryableExtensions;

namespace StateOfNeo.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly StateOfNeoContext db;

        public TransactionService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public T Find<T>(string blockHash) =>
            this.db.Transactions
                .Where(x => x.Block.Hash == blockHash)
                .ProjectTo<T>()
                .FirstOrDefault();

        public decimal TotalClaimed() =>
            this.db.Transactions
                .Any(x => x.Type == TransactionType.ClaimTransaction)
            ? this.db.Transactions
                .Include(x => x.Assets).ThenInclude(x => x.Asset)
                .Where(x => x.Type == TransactionType.ClaimTransaction)
                .SelectMany(x => x.Assets.Where(a => a.AssetType == GlobalAssetType.Gas))
                .Sum(x => x.Amount)
            : 0;
    }
}
