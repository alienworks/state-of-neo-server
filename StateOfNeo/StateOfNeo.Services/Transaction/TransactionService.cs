using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using StateOfNeo.Data;
using StateOfNeo.Data.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace StateOfNeo.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly StateOfNeoContext db;

        public TransactionService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public Data.Models.Transactions.Transaction Find(string hash) =>
            this.db.Transactions
                .Include(x => x.Block)
                .Include(x => x.Assets)
                .Include(x => x.Attributes)
                .Include(x => x.Witnesses)
                .Where(x => x.ScriptHash == hash)
                .FirstOrDefault();
    }
}
