using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class TransactionWitness : BaseEntity
    {
        public int Id { get; set; }

        public string InvocationScriptAsHexString { get; set; }

        public string VerificationScriptAsHexString { get; set; }

        public string Address { get; set; }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
