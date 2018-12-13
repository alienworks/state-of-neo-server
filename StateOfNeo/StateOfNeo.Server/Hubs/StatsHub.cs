using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Services;
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
            await this.Clients.Caller.SendAsync("header", this.state.MainStats.GetHeaderStats());
            // Blocks
            await this.Clients.All.SendAsync("total-block-count", this.state.MainStats.GetTotalBlocksCount());
            await this.Clients.All.SendAsync("total-block-time", this.state.MainStats.GetTotalBlocksTimesCount());
            await this.Clients.All.SendAsync("total-block-size", this.state.MainStats.GetTotalBlocksSizesCount());

            await this.Clients.Caller.SendAsync("tx-count", this.state.MainStats.GetTotalTxCount());
            await this.Clients.Caller.SendAsync("address-count", this.state.MainStats.GetTotalAddressCount());
            await this.Clients.Caller.SendAsync("assets-count", this.state.MainStats.GetTotalAssetsCount());
            await this.Clients.Caller.SendAsync("total-claimed", this.state.MainStats.GetTotalClaimed());
        }
    }
}
