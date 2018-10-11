using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace StateOfNeo.Node.Hubs
{
    public interface IFailedP2PHub
    {
        Task Send();
    }

    public class FailedP2PHub : Hub<IFailedP2PHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
