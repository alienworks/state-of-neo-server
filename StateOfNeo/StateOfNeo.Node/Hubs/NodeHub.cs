using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace StateOfNeo.Node.Hubs
{
    public interface INoteHub
    {
        Task Send();
    }

    public class NodeHub : Hub<INoteHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
