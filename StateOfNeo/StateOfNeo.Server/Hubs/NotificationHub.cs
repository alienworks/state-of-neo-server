using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Common;
using StateOfNeo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IStateService state;

        public NotificationHub(IStateService state)
        {
            this.state = state;
        }

        public override async Task OnConnectedAsync()
        {
            await this.Clients.Caller.SendAsync("all", 
                this.state.GetNotificationsForContract(NotificationConstants.AllNotificationsKey));
        }

        public async Task TrackContract(string hash)
        {
            var contractHash = this.CheckAndGetProperContractHash(hash);
            if (contractHash != null)
            {
                await this.Groups.AddToGroupAsync(this.Context.ConnectionId, contractHash);
                await this.Clients.Group(contractHash).SendAsync("contract", this.state.GetNotificationsForContract(contractHash));
            }
            else
            {
                await this.Clients.Caller.SendAsync("hash-error", $"Hash : {hash} is with wrong value or length please enter new one");
            }
        }

        public async Task Unsubscribe(string hash)
        {
            var contractHash = this.CheckAndGetProperContractHash(hash);
            if (contractHash != null)
            {
                await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, contractHash);
                await this.Clients.Caller.SendAsync("unsubscribed", contractHash);
                await this.Clients.All.SendAsync("all",
                    this.state.GetNotificationsForContract(NotificationConstants.AllNotificationsKey));
            }
            else
            {
                await this.Clients.Caller.SendAsync("hash-error", $"Hash : {hash} is with wrong value or length please enter new one");
            }
        }

        private string CheckAndGetProperContractHash(string hash)
        {
            if (!hash.StartsWith("0x"))
            {
                hash = "0x" + hash;
            }

            if (hash.Length != 42)
            {
                return null;
            }

            return hash;
        }
    }
}
