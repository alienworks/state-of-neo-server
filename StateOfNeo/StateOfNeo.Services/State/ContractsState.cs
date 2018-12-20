using Neo;
using Neo.Ledger;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Hub;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Services
{
    public class ContractsState : IContractsState
    {
        private readonly StateOfNeoContext db;

        private readonly IDictionary<string, List<NotificationHubViewModel>> contractsNotifications =
            new Dictionary<string, List<NotificationHubViewModel>>();

        public ContractsState()
        {
        }

        public IEnumerable<NotificationHubViewModel> GetNotificationsFor(string hash)
        {
            if (!this.contractsNotifications.ContainsKey(hash))
            {
                return null;
            }

            return this.contractsNotifications[hash];
        }

        public void SetOrAddNotificationsForContract(string key, string hash, long timestamp, string type, string[] values)
        {
            var newValue = new NotificationHubViewModel(timestamp, hash, type, values);
            if (!this.contractsNotifications.ContainsKey(key))
            {
                UpdateNotificationContractInfo(hash, newValue);
                this.contractsNotifications.Add(key, new List<NotificationHubViewModel> { newValue });
            }
            else
            {
                if (key != NotificationConstants.AllNotificationsKey)
                {
                    var existingNotification = this.contractsNotifications[key].First();
                    newValue.SetContractInfo(existingNotification.ContractName, existingNotification.ContractAuthor);
                }
                else
                {
                    UpdateNotificationContractInfo(hash, newValue);
                }

                this.contractsNotifications[key].Insert(0, newValue);

                if (this.contractsNotifications[key].Count > NotificationConstants.MaxNotificationCount)
                {
                    this.contractsNotifications[key] =
                        this.contractsNotifications[key].Take(NotificationConstants.MaxNotificationCount).ToList();
                }

                if (key != NotificationConstants.AllNotificationsKey)
                {
                    this.SetOrAddNotificationsForContract(NotificationConstants.AllNotificationsKey, hash, timestamp, type, values);
                }
            }
        }

        private static void UpdateNotificationContractInfo(string hash, NotificationHubViewModel newValue)
        {
            var scripthash = UInt160.Parse(hash);
            var contract = Blockchain.Singleton.Store.GetContracts().TryGet(scripthash);
            var contractName = contract.Name;
            var contractAuthor = contract.Author;

            newValue.SetContractInfo(contractName, contractAuthor);
        }

    }
}
