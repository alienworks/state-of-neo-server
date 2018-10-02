using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class Address : BaseEntity
    {
        public Address()
        {
            this.Transactions = new HashSet<Transaction>();
        }

        [Key]
        public string PublicAddress { get; set; }
        
        public ICollection<Transaction> Transactions { get; set; }


    }
}
