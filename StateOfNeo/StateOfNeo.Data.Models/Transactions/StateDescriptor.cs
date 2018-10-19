using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class StateDescriptor : BaseEntity
    {
        public int Id { get; set; }

        public StateType Type { get; set; }

        public string KeyAsHexString { get; set; }

        public string Field { get; set; }

        public string ValueAsHexString { get; set; }

        public int TransactionId { get; set; }

        public virtual StateTransaction Transaction { get; set; }
    }
}
