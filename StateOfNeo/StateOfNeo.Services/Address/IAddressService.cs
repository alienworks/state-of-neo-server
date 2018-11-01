using StateOfNeo.Common.Enums;
using StateOfNeo.ViewModels.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Services.Address
{
    public interface IAddressService
    {
        int ActiveAddressesInThePastThreeMonths();

        int CreatedAddressesPer(TimePeriod timePeriod);

        int CreatedAddressesCount();

        IEnumerable<PublicAddressListViewModel> TopOneHundredNeo();

        IEnumerable<PublicAddressListViewModel> TopOneHundredGas();
    }
}
