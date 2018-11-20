using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Data;
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
            var block = this.db.Blocks
                .OrderByDescending(x => x.Height)
                .ProjectTo<BlockHubViewModel>()
                .FirstOrDefault();
            await this.Clients.All.SendAsync("header", block);

            var txCount = this.db.Transactions.Count();
            await this.Clients.All.SendAsync("tx-count", txCount);
            
            var addrCount = this.db.Addresses.Count();
            await this.Clients.All.SendAsync("address-count", addrCount);

            var assetsCount = this.db.Assets.Count();
            await this.Clients.All.SendAsync("assets-count", assetsCount);
        }
    }
}
