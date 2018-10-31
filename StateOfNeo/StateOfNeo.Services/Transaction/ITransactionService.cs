using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Services.Transaction
{
    public interface ITransactionService
    {
        Data.Models.Transactions.Transaction Find(string hash);
    }
}
