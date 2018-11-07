using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class TransactionInputOutput : BaseEntity
    {
        public decimal Value { get; set; }


        public string TransactionScriptHash { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
