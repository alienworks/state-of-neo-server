using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Address
{
    public class AddressListViewModel
    {
        public string Address { get; set; }

        public DateTime Created { get; set; }

        public int Transactions { get; set; }

        public DateTime LastTransactionTime { get; set; }

        public IEnumerable<AddressAssetViewModel> Balances { get; set; }
    }
}
