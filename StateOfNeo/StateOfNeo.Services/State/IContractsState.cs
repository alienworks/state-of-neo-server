using StateOfNeo.ViewModels.Hub;
using System.Collections.Generic;

namespace StateOfNeo.Services
{
    public interface IContractsState
    {
        IEnumerable<NotificationHubViewModel> GetNotificationsFor(string hash);
        void SetOrAddNotificationsForContract(string key, string hash, long timestamp, string type, string[] values);
    }
}
