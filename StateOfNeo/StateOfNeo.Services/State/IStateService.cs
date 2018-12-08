using StateOfNeo.ViewModels;

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
    }
}
