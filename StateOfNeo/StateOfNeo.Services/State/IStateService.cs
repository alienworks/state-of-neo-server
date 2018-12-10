using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Hub;
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

        IEnumerable<NotificationHubViewModel> GetNotificationsForContract(string hash);
        void SetOrAddNotificationsForContract(string hash, long timestamp, string[] values);
    }
}
