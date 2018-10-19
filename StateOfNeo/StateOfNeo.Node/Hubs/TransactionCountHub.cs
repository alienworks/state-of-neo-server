using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

namespace StateOfNeo.Node.Hubs
{
    public interface ITransactionCountHub
    {
        Task Send();
    }

    public class TransactionCountHub : Hub<ITransactionCountHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
