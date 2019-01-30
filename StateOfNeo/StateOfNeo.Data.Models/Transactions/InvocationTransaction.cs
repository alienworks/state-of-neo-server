using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StateOfNeo.Data.Models.Transactions
{
    public class InvocationTransaction : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string ScriptAsHexString { get; set; }

        public decimal Gas { get; set; }

        public string ContractHash { get; set; }

        public int? SmartContractId { get; set; }

        public virtual SmartContract SmartContract { get; set; }
        
        public string TransactionHash { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
