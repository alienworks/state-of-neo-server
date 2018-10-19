using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class RegisterTransaction : BaseEntity
    {
        public int Id { get; set; }

        public AssetType AssetType { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }

        public byte Precision { get; set; }

        public string OwnerPublicKey { get; set; }

        public string AdminAddress { get; set; }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
