using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class Address : BaseEntity
    {
        public Address()
        {
            this.OutgoingTransactions = new HashSet<Transaction>();
            this.IncomingTransactions = new HashSet<Transaction>();
        }

        [Key]
        public string PublicAddress { get; set; }

        [InverseProperty(nameof(Transaction.FromAddress))]
        public ICollection<Transaction> OutgoingTransactions { get; set; }

        [InverseProperty(nameof(Transaction.ToAddress))]
        public ICollection<Transaction> IncomingTransactions { get; set; }

    }
}
