using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class StateTransaction : BaseEntity
    {
        public int Id { get; set; }
        
        public StateTransaction()
        {
            this.Descriptors = new HashSet<StateDescriptor>();
        }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }

        public virtual ICollection<StateDescriptor> Descriptors { get; set; }
    }
}
