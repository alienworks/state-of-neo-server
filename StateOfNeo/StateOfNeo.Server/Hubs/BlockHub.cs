using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Neo.Ledger;
using StateOfNeo.Data;
using StateOfNeo.ViewModels;

namespace StateOfNeo.Server.Hubs
{
    public class BlockHub : Hub
    {
        private readonly StateOfNeoContext db;

        public BlockHub(StateOfNeoContext db)
        {
            this.db = db;
        }

        public override async Task OnConnectedAsync()
        {
            var block = this.db.Blocks
                .OrderByDescending(x => x.Height)
                .ProjectTo<HeaderStatsViewModel>()
                .FirstOrDefault();
            
            await this.Clients.All.SendAsync("Receive", block);
        }
    }
}
