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
            CreateWebHostBuilder(args).Build().Run();

            InitializeNeoSystem();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void InitializeNeoSystem()
        {
            using (LevelDBStore store = new LevelDBStore(NeoSettings.Default.DataDirectoryPath))
            using (NeoSystem = new NeoSystem(store))
            {
                NeoSystem.StartNode(NeoSettings.Default.NodePort, NeoSettings.Default.WsPort);
            }
        }
    }
}
