using Neo.Network.P2P.Payloads;
using StateOfNeo.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionListViewModel : IComparable<TransactionListViewModel>, IEquatable<TransactionListViewModel>
    {
        public string Hash { get; set; }

        public int Size { get; set; }

        public TransactionType Type { get; set; }

        public long Timestamp { get; set; }

        public DateTime FinalizedAt => this.Timestamp.ToUnixDate();

        public int CompareTo(TransactionListViewModel other)
        {
            return this.Hash.CompareTo(other.Hash);
        }

        public bool Equals(TransactionListViewModel other)
        {
            return this.Hash.Equals(other.Hash);
        }

        public override int GetHashCode()
        {
            return this.Hash.GetHashCode();
        }
    }
}
