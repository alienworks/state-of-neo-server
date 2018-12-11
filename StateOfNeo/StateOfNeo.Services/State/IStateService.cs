using StateOfNeo.Common.Enums;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;

namespace StateOfNeo.Services
{
    public interface IStateService
    {
        HeaderStatsViewModel GetHeaderStats();
        void SetHeaderStats(HeaderStatsViewModel newValue);

        long GetTotalTxCount();
        void AddToTotalTxCount(int count);

        int GetTotalAddressCount();
        void AddTotalAddressCount(int count);

        int GetTotalAssetsCount();
        void AddTotalAssetsCount(int count);

        decimal GetTotalClaimed();
        void AddTotalClaimed(decimal amount);

        ICollection<ChartStatsViewModel> GetTransactionsChart(UnitOfTime unitOfTime, int count);
        void AddTransactions(int count, DateTime time);
    }
}
