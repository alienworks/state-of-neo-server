using StateOfNeo.Data.Models.Transactions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class Block : StampedEntity
    {
        public Block()
        {
            this.Transactions = new HashSet<Transaction>();
            this.NodeStatusUpdates = new HashSet<NodeStatus>();
        }

        [Key]
        public string Hash { get; set; }

        public int Height { get; set; }

        public int Size { get; set; }

        public double TimeInSeconds { get; set; }

        public ulong ConsensusData { get; set; }

        public string NextConsensusNodeAddress { get; set; }

        public string Validator { get; set; }

        public string InvocationScript { get; set; }

        public string VerificationScript { get; set; }

        public string PreviousBlockHash { get; set; }

        public virtual Block PreviousBlock { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual ICollection<NodeStatus> NodeStatusUpdates { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"Hash: {Hash}\n" +
                            $"CreatedOn: {CreatedOn}\n" +
                            $"TimeStamp: {Timestamp}\n" +
                            $"MonthlyStamp: {MonthlyStamp}\n" +
                            $"DailyStamp: {DailyStamp}\n" +
                            $"Height: {Height}\n" +
                            $"Size: {Size}\n" +
                            $"TimeInSeconds: {TimeInSeconds}\n" +
                            $"ConsensusData: {ConsensusData}\n" +
                            $"NextConsensusNodeAddress: {NextConsensusNodeAddress}\n" +
                            $"Validator: {Validator}\n" +
                            $"InvocationScript: {InvocationScript}\n" +
                            $"VerificationScript: {VerificationScript}\n" +
                            $"PreviousBlockHash: { PreviousBlockHash}\n");

            foreach (var item in Transactions)
            {
                result.AppendLine($"{item.ToString()}");
            }
            return result.ToString();
        }
    }
}