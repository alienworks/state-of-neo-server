using Microsoft.Extensions.Hosting;
using Neo.Ledger;
using Neo.Network.P2P;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateOfNeo.Services
{
    public class SyncingService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Height: {Blockchain.Singleton.Height.ToString()}/{Blockchain.Singleton.HeaderHeight.ToString()}");
                Console.WriteLine($"Connections: {LocalNode.Singleton.GetRemoteNodes().ToList().Count().ToString()}");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
