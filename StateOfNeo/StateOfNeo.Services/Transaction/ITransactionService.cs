using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;
using System.Text;
using X.PagedList;

namespace StateOfNeo.Services.Transaction
{
    public interface ITransactionService
    {
        T Find<T>(string hash);

        decimal TotalClaimed();

        IPagedList<T> GetPageTransactions<T>(int page = 1, int pageSize = 10, string blockHash = null);
        
        IPagedList<T> TransactionsForAddress<T>(string address, int page = 1, int pageSize = 10);

        IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter);
        IEnumerable<ChartStatsViewModel> GetPieStats();
    }
}
