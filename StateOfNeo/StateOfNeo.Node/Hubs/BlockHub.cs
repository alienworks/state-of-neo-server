using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace StateOfNeo.Node.Hubs
{
    public interface IBlockHub
    {
        Task Send();
    }

    public class BlockHub : Hub<IBlockHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
