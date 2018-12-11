namespace StateOfNeo.ViewModels.Hub
{
    public class NotificationHubViewModel
    {
        public long Timestamp { get; set; }
        public string ContractHash { get; set; }
        public string[] Values { get; set; }

        public NotificationHubViewModel(long timestamp, string contractHash, string[] values)
        {
            this.Timestamp = timestamp;
            this.ContractHash = contractHash;
            this.Values = values;
        }
    }
}
