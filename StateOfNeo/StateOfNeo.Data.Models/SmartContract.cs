﻿using Neo.SmartContract;
using StateOfNeo.Data.Models.Transactions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class SmartContract : BaseEntity
    {
        public SmartContract()
        {
            this.InvocationTransactions = new HashSet<InvocationTransaction>();
        }

        [Key]
        public int Id { get; set; }

        public string Hash { get; set; }

        public long Timestamp { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public string Email { get; set; }

        public bool HasStorage { get; set; }

        public bool Payable { get; set; }

        public bool HasDynamicInvoke { get; set; }

        public string InputParameters { get; set; }

        public ContractParameterType ReturnType { get; set; }

        public virtual IEnumerable<InvocationTransaction> InvocationTransactions { get; set; }
    }
}
