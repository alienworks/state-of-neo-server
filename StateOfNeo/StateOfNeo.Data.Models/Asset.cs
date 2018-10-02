using StateOfNeo.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class Asset : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public GlobalAssetType Type { get; set; }

        public int MaxSupply { get; set; }


    }
}
