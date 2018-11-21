using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Data;
using StateOfNeo.Server.Actors;
using StateOfNeo.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public class StatsHub : Hub
    {
        private readonly StateOfNeoContext db;

        public StatsHub(StateOfNeoContext db)
        {
            this.db = db;
        }

        public override async Task OnConnectedAsync()
        {
            await this.Clients.All.SendAsync("header", BlockPersister.HeaderStats);
            await this.Clients.All.SendAsync("tx-count", BlockPersister.TotalTxCount);
            await this.Clients.All.SendAsync("address-count", BlockPersister.TotalAddressCount);
            await this.Clients.All.SendAsync("assets-count", BlockPersister.TotalAssetsCount);
            await this.Clients.All.SendAsync("total-claimed", BlockPersister.TotalClaimed);
        }
    }
}
