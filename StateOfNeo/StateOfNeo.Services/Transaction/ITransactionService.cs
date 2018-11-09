using StateOfNeo.Common.Enums;
using StateOfNeo.ViewModels.Chart;
using StateOfNeo.ViewModels.Transaction;
using System.Collections.Generic;
using X.PagedList;

namespace StateOfNeo.Services.Transaction
{
    public interface ITransactionService
    {
        T Find<T>(string hash);

        IPagedList<T> GetPageTransactions<T>(int page = 1, int pageSize = 10, string blockHash = null);
        
        IPagedList<TransactionListViewModel> TransactionsForAddress(string address, int page = 1, int pageSize = 10);

        IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter);
        IEnumerable<ChartStatsViewModel> GetPieStats();

        double AveragePer(UnitOfTime unitOfTime);
        long Total();
        decimal TotalClaimed();
    }
}
