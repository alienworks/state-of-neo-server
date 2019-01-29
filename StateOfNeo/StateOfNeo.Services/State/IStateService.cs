using StateOfNeo.Common.Enums;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Hub;
using System.Collections.Generic;
using StateOfNeo.ViewModels.Chart;
using System;
using StateOfNeo.ViewModels.Address;
using StateOfNeo.ViewModels.Transaction;
using X.PagedList;

namespace StateOfNeo.Services
{
    public interface IStateService
    {
        IMainStatsState MainStats { get; }
        IContractsState Contracts { get; }

        IEnumerable<ChartStatsViewModel> GetTransactionTypes();

        ICollection<ChartStatsViewModel> GetTransactionsChart(UnitOfTime unitOfTime, int count);
        void AddTransactions(int count, DateTime time);

        ICollection<ChartStatsViewModel> GetAddressesChart(UnitOfTime unitOfTime, int count);
        void AddAddresses(int count, DateTime time);

        ICollection<ChartStatsViewModel> GetBlockSizesChart(UnitOfTime unitOfTime, int count);
        void AddBlockSize(int size, DateTime time);

        ICollection<ChartStatsViewModel> GetBlockTimesChart(UnitOfTime unitOfTime, int count);
        void AddBlockTime(double blockSeconds, DateTime time);

        ICollection<ChartStatsViewModel> GetBlockTransactionsChart(UnitOfTime unitOfTime, int count);
        void AddBlockTransactions(int transactions, DateTime time);

        void AddActiveAddress(IEnumerable<AddressListViewModel> addresses);
        IPagedList<AddressListViewModel> GetAddressesPage(int page = 1, int pageSize = 10);

        void AddToTransactionsList(TransactionListViewModel tx);
        void AddToTransactionsList(IEnumerable<TransactionListViewModel> txs);
        IPagedList<TransactionListViewModel> GetTransactionsPage(int page = 1, int pageSize = 10, string type = null);

        void AddToDetailedTransactionsList(TransactionDetailedListViewModel tx);
        void AddToDetailedTransactionsList(IEnumerable<TransactionDetailedListViewModel> txs);
        IPagedList<TransactionDetailedListViewModel> GetDetailedTransactionsPage(int page = 1, int pageSize = 10, string type = null);

    }
}
