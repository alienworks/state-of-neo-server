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
            using (LevelDBStore store = new LevelDBStore(NeoSettings.Default.DataDirectoryPath))
            using (NeoSystem = new NeoSystem(store))
            {
                CreateWebHostBuilder(args).Build().Run();
                NeoSystem.StartNode(NeoSettings.Default.NodePort, NeoSettings.Default.WsPort);
            }
        }
    }
}
