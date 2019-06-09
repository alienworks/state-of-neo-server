using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Server.Cache;
using StateOfNeo.Services;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public class PeersHub : Hub
    {
        private readonly NodeCache nodeCache;

        public PeersHub(NodeCache nodeCache)
        {
            this.nodeCache = nodeCache;
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

        public async Task InitInfoByType(IClientProxy proxy, int pageSize = 10)
        {
            await proxy.SendAsync("list", this.nodeCache.GetCachedPeers<PeerViewModel>());
            await proxy.SendAsync("total-found", this.nodeCache.PeersCollected.Count);
            await proxy.SendAsync("total-tracked", this.nodeCache.GetCachedPeersCount);
        }
    }
}
