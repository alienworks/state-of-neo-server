﻿using StateOfNeo.Common.Enums;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Hub;
using System.Collections.Generic;
using StateOfNeo.ViewModels.Chart;
using System;

namespace StateOfNeo.Services
{
    public interface IStateService
    {
        IMainStatsState MainStats { get; }
        IContractsState Contracts { get; }
        
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
    }
}
