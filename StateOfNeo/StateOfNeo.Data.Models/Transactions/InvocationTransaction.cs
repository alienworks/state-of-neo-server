using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class InvocationTransaction : BaseEntity
    {
        public int Id { get; set; }

        public string ScriptAsHexString { get; set; }

        public decimal Gas { get; set; }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
