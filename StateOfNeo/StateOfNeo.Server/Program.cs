using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Neo;
using Neo.Persistence.LevelDB;

namespace StateOfNeo.Server
{
    public class Program
    {
        public static NeoSystem NeoSystem;

        public static void Main(string[] args)
        {
            InitializeNeoSystem(args);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void InitializeNeoSystem(string[] args)
        {
            LevelDBStore store = new LevelDBStore(NeoSettings.Default.DataDirectoryPath);
            NeoSystem = new NeoSystem(store);
            
            NeoSystem.StartNode(NeoSettings.Default.NodePort, NeoSettings.Default.WsPort);
            CreateWebHostBuilder(args).Build().Run();
        }
    }
}
