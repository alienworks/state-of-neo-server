using StateOfNeo.Common.Enums;
using StateOfNeo.ViewModels.Address;
using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;
using System.Text;
using X.PagedList;

namespace StateOfNeo.Services.Address
{
    public interface IAddressService
    {
        T Find<T>(string address);

        IPagedList<AddressListViewModel> GetPage(int page = 1, int pageSize = 10);

        int ActiveAddressesInThePastThreeMonths();

        int CreatedAddressesPer(UnitOfTime timePeriod);

        int CreatedAddressesCount();
        int CreatedAddressesCountForLast(UnitOfTime unit = UnitOfTime.Day);

        IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter);
        IEnumerable<ChartStatsViewModel> GetTransactionStats(string address);

        IEnumerable<AddressListViewModel> TopOneHundredNeo();

        IEnumerable<AddressListViewModel> TopOneHundredGas();
    }
}
