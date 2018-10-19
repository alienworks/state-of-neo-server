using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class EnrollmentTransaction : BaseEntity
    {
        public int Id { get; set; }

        public string PublicKey { get; set; }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
