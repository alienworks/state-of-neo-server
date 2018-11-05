using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Services.Transaction
{
    public interface ITransactionService
    {
        T Find<T>(string hash);

        decimal TotalClaimed();

        IEnumerable<ChartStatsViewModel> GetStats(ChartFilterViewModel filter);
        IEnumerable<ChartStatsViewModel> GetPieStats();
    }
}
