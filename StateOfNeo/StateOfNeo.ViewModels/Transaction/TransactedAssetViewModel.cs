using StateOfNeo.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactedAssetViewModel
    {
        public decimal Amount { get; set; }

        public AssetType AssetType { get; set; }

        public string FromAddress { get; set; }

        public string ToAddress { get; set; }
    }
}
