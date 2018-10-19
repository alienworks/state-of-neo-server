using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class TransactionAttribute : BaseEntity
    {
        public int Id { get; set; }

        public int Usage { get; set; }

        public string DataAsHexString { get; set; }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
