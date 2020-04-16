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
            this.AddressesInTransaction = new HashSet<AddressInTransaction>();
            this.AddressesInAssetTransactions = new HashSet<AddressInAssetTransaction>();
            this.Balances = new HashSet<AddressAssetBalance>();
            this.OutgoingTransactions = new HashSet<TransactedAsset>();
            this.IncomingTransactions = new HashSet<TransactedAsset>();
        }

        [Key]
        public string PublicAddress { get; set; }

        public DateTime FirstTransactionOn { get; set; }

        public DateTime LastTransactionOn { get; set; }

        public long LastTransactionStamp { get; set; }

        public int TransactionsCount { get; set; }

        [InverseProperty(nameof(TransactedAsset.FromAddress))]
        public ICollection<TransactedAsset> OutgoingTransactions { get; set; }

        [InverseProperty(nameof(TransactedAsset.ToAddress))]
        public ICollection<TransactedAsset> IncomingTransactions { get; set; }

        public virtual ICollection<AddressAssetBalance> Balances { get; set; }

        public virtual ICollection<AddressInTransaction> AddressesInTransaction { get; set; }

        public virtual ICollection<AddressInAssetTransaction> AddressesInAssetTransactions { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"Address: {PublicAddress}\n" +
                            $"CreatedOn: {CreatedOn}\n" +
                            $"FirstTransactionOn: {FirstTransactionOn}\n" +
                            $"LastTransactionOn: {LastTransactionOn}\n" +
                            $"LastTransactionStamp: {LastTransactionStamp}\n" +
                            $"TransactionsCount: { TransactionsCount}\n");

            foreach (var item in Balances)
            {
                result.AppendLine($"{item.ToString()}");
            }
            return result.ToString();
        }
    }
}