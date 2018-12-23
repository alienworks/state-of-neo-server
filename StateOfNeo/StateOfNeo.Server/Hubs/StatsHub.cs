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
            await this.InitInfo("caller");
        }

        public async Task InitInfo(string type = null)
        {
            if (type == "clients")
            {
                await this.InitInfoByType(this.Clients.All);
            }
            else if (type == "caller")
            {
                await this.InitInfoByType(this.Clients.Caller);
            }
            else
            {
                await this.InitInfoByType(this.Clients.All);
            }
        }

        public async Task InitInfoByType(IClientProxy proxy)
        {
            await proxy.SendAsync("header", this.state.MainStats.GetHeaderStats());
            await proxy.SendAsync("total-block-count", this.state.MainStats.GetTotalBlocksCount());
            await proxy.SendAsync("total-block-time", this.state.MainStats.GetTotalBlocksTimesCount());
            await proxy.SendAsync("total-block-size", this.state.MainStats.GetTotalBlocksSizesCount());
            await proxy.SendAsync("tx-count", this.state.MainStats.GetTotalTxCount());
            await proxy.SendAsync("total-claimed", this.state.MainStats.GetTotalClaimed());
            await proxy.SendAsync("gas-neo-tx-count", this.state.MainStats.GetTotalGasAndNeoTxCount());
            await proxy.SendAsync("nep-5-tx-count", this.state.MainStats.GetTotalNep5TxCount());
            await proxy.SendAsync("address-count", this.state.MainStats.GetTotalAddressCount());
            await proxy.SendAsync("assets-count", this.state.MainStats.GetTotalAssetsCount());
        }
    }
}
