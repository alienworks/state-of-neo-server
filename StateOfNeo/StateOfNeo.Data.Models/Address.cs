using StateOfNeo.Data.Models.Transactions;
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
            this.OutgoingTransactions = new HashSet<TransactedAsset>();
            this.IncomingTransactions = new HashSet<TransactedAsset>();
        }

        [Key]
        public string PublicAddress { get; set; }

        [InverseProperty(nameof(TransactedAsset.FromAddress))]
        public ICollection<TransactedAsset> OutgoingTransactions { get; set; }

        [InverseProperty(nameof(TransactedAsset.ToAddress))]
        public ICollection<TransactedAsset> IncomingTransactions { get; set; }

    }
}
