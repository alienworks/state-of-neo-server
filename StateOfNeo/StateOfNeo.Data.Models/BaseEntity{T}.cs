using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public abstract class BaseEntity
    {
        public DateTime CreatedOn { get; set; }
    }
}
