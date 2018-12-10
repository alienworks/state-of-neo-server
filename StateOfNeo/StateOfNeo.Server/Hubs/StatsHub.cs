using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Data;
using StateOfNeo.Server.Actors;
using StateOfNeo.Services;
using StateOfNeo.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public class StatsHub : Hub
    {
        private readonly IStateService state;

        public StatsHub(IStateService state)
        {
            this.state = state;
        }

        public override async Task OnConnectedAsync()
        {
            await this.Clients.All.SendAsync("header", this.state.GetHeaderStats());
            await this.Clients.All.SendAsync("tx-count", this.state.GetTotalTxCount());
            await this.Clients.All.SendAsync("address-count", this.state.GetTotalAddressCount());
            await this.Clients.All.SendAsync("assets-count", this.state.GetTotalAssetsCount());
            await this.Clients.All.SendAsync("total-claimed", this.state.GetTotalClaimed());
        }
    }
}
