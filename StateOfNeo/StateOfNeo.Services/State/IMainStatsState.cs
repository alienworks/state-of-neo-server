using StateOfNeo.ViewModels;
using System.Numerics;

namespace StateOfNeo.Services
{
    public interface IMainStatsState
    {
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

        int GetTotalBlocksCount();
        void AddTotalBlocksCount(int count);
        double GetTotalBlocksTimesCount();
        void AddToTotalBlocksTimesCount(double value);
        long GetTotalBlocksSizesCount();
        void AddToTotalBlocksSizesCount(int value);
    }
}
