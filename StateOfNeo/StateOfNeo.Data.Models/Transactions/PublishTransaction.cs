using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models.Transactions
{
    public class PublishTransaction : BaseEntity
    {
        public int Id { get; set; }

        public string ScriptAsHexString { get; set; }

        public string ParameterList { get; set; }

        public string ReturnType { get; set; }

        public bool NeedStorage { get; set; }

        public string Name { get; set; }

        public string CodeVersion { get; set; }

        public string Author { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public int TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
