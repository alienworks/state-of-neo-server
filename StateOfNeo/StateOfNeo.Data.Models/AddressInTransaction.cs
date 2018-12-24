using StateOfNeo.Data.Models.Transactions;

namespace StateOfNeo.Data.Models
{
    public class AddressInTransaction : StampedEntity
    {
        public int Id { get; set; }

        public float Amount { get; set; }

        public string AddressPublicAddress { get; set; }

        public virtual Address Address { get; set; }

        public string TransactionHash { get; set; }

        public virtual Transaction Transaction { get; set; }

        public string AssetHash { get; set; }

        public virtual Asset Asset { get; set; }
    }
}
