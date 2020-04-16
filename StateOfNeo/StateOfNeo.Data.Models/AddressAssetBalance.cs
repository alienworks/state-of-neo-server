using System.Text;

namespace StateOfNeo.Data.Models
{
    public class AddressAssetBalance : BaseEntity
    {
        public int Id { get; set; }

        public decimal Balance { get; set; }

        public int TransactionsCount { get; set; }

        public string AddressPublicAddress { get; set; }

        public virtual Address Address { get; set; }

        public string AssetHash { get; set; }

        public virtual Asset Asset { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"Id: {Id}\n" +
                $"Balance: {Balance}\n" +
                $"TransactionsCount: {TransactionsCount}\n" +
                $"AddressPublicAddress: {AddressPublicAddress}\n" +
                $"AssetHash: {AssetHash}\n" +
                $"TransactionsCount: {TransactionsCount}\n");

            return result.ToString();
        }
    }
}