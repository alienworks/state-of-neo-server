using StateOfNeo.ViewModels.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Address
{
    public class AddressDetailsViewModel
    {
        public string Address { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastTransactionTime { get; set; }

        public IEnumerable<AddressAssetViewModel> Balances { get; set; }

        public IEnumerable<TransactionListViewModel> Transactions { get; set; }
    }
}
