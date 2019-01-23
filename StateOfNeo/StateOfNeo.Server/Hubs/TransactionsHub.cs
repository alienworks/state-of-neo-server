using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public class TransactionsHub : Hub
    {
        private readonly IStateService state;

        public TransactionsHub(IStateService state)
        {
            this.state = state;
        }

        public override async Task OnConnectedAsync()
        {
            await this.InitInfo("caller");
        }

        public async Task InitInfo(string type = null, int pageSize = 10)
        {
            if (type == "clients")
            {
                await this.InitInfoByType(this.Clients.All, pageSize);
            }
            else if (type == "caller")
            {
                await this.InitInfoByType(this.Clients.Caller, pageSize);
            }
            else
            {
                await this.InitInfoByType(this.Clients.All, pageSize);
            }
        }

        public async Task InitInfoByType(IClientProxy proxy, int pageSize = 10)
        {
            await proxy.SendAsync("list", this.state.GetTransactionsPage(1, pageSize));
        }
    }
}
