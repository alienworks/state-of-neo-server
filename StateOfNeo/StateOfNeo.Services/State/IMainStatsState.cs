using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System.Numerics;

namespace StateOfNeo.Services
{
    public interface IMainStatsState
    {
        TotalStats TotalStats { get; set; }

        HeaderStatsViewModel GetHeaderStats();
        void SetHeaderStats(HeaderStatsViewModel newValue);

        long GetTotalTxCount();
        void AddToTotalTxCount(int count);
        decimal GetTotalClaimed();
        void AddTotalClaimed(decimal amount);

        int GetTotalAddressCount();
        void AddTotalAddressCount(int count);

        int GetTotalAssetsCount();
        void AddTotalAssetsCount(int count);
        long GetTotalGasAndNeoTxCount();
        void AddToTotalNeoGasTxCount(int value);
        long GetTotalNep5TxCount();
        void AddToTotalNep5TxCount(int value);

        int GetTotalBlocksCount();
        void AddTotalBlocksCount(int count);
        decimal GetTotalBlocksTimesCount();
        void AddToTotalBlocksTimesCount(decimal value);
        long GetTotalBlocksSizesCount();
        void AddToTotalBlocksSizesCount(int value);
    }
}
