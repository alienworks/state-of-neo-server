using StateOfNeo.Common.Enums;
using StateOfNeo.ViewModels.Address;
using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Services.Address
{
    public interface IAddressService
    {
        T Find<T>(string address);

        int ActiveAddressesInThePastThreeMonths();

        int CreatedAddressesPer(UnitOfTime timePeriod);

        int CreatedAddressesCount();

        IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter);

        IEnumerable<AddressListViewModel> TopOneHundredNeo();

        IEnumerable<AddressListViewModel> TopOneHundredGas();
    }
}
