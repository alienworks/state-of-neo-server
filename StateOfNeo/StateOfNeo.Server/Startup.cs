using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Seed;
using StateOfNeo.Infrastructure.Mapping;
using StateOfNeo.Server.Actors;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Common;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.Services;
using StateOfNeo.Services.Address;
using StateOfNeo.Services.Block;
using StateOfNeo.Services.Transaction;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server
{
    public class Startup
    {
        public ILoggerFactory LoggerFactory { get; set; }
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .CreateLogger();

            AutoMapperConfig.Init();

            services.AddResponseCaching();

            services.Configure<NetSettings>(this.Configuration.GetSection("NetSettings"));
            services.Configure<ImportBlocksSettings>(this.Configuration.GetSection("Plugins.ImportBlocks"));
            services.Configure<DbSettings>(this.Configuration.GetSection("ConnectionStrings"));

            // Data.Services
            services.AddScoped<IPaginatingService, PaginatingService>();
            services.AddScoped<INodeService, NodeService>();
            services.AddScoped<IBlockService, BlockService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ISmartContractService, SmartContractService>();
            services.AddScoped<IPeerService, PeerService>();

            services.AddSingleton<IMainStatsState, MainStatsState>();
            services.AddSingleton<IStateService, StateService>();
            //services.AddSingleton<BalanceUpdater>(); 
            services.AddSingleton<BlockchainBalances>();
            services.AddSingleton<SmartContractEngine>(); 
            services.AddSingleton<AssetsCreatorUpdate>(); 

            // Infrastructure
            services.AddSingleton<NodeCache>();
            services.AddScoped<RPCNodeCaller>();
            services.AddScoped<PeersEngine>();
            services.AddScoped<LocationCaller>();

            services
                .AddDbContext<StateOfNeoContext>(options => options.UseSqlServer(
                    this.Configuration.GetConnectionString("DefaultConnection"),
                    opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(100).TotalSeconds)))
                .AddEntityFrameworkSqlServer();

            services.AddTransient<StateOfNeoSeedData>();

            services.AddCors();
            services.AddSignalR();
            services
                .AddMvc(options =>
                {
                    //options.SslPort = 5001;
                    //options.Filters.Add(new RequireHttpsAttribute());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }


        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            SmartContractEngine smartContractEngine,
            StateOfNeoSeedData seeder,
            StateOfNeoContext ctx,
            IServiceProvider services,
            IOptions<NetSettings> netSettings,
            IOptions<ImportBlocksSettings> importSettings,
            IHubContext<StatsHub> statsHub,
            IHubContext<PeersHub> peersHub,
            IHubContext<TransactionsHub> txHub,
            IHubContext<NotificationHub> notificationHub,
            BlockchainBalances blockChainBalances,
            RPCNodeCaller nodeCaller,
            NodeCache nodeCache,
            IStateService state,
            AssetsCreatorUpdate assetsCreatorUpdate
            )
        {
            assetsCreatorUpdate.Run().Wait();

            nodeCache.AddPeerToCache(ctx.Peers.ToList());
            var connectionString = this.Configuration.GetConnectionString("DefaultConnection");

            Program.NeoSystem.ActorSystem.ActorOf(BlockPersister.Props(
                connectionString,
                state,
                statsHub,
                txHub,
                notificationHub,
                blockChainBalances,
                netSettings.Value.Net));

            Program.NeoSystem.ActorSystem.ActorOf(NodePersister.Props(
                connectionString,
                netSettings.Value.Net,
                nodeCaller));

            Program.NeoSystem.ActorSystem.ActorOf(NodeFinder.Props(
                 connectionString,
                 netSettings.Value,
                 peersHub,
                 nodeCache));

            //    Program.NeoSystem.ActorSystem.ActorOf(NotificationsListener.Props(Program.NeoSystem.Blockchain, connectionString));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(builder =>
            {
                builder
                    .WithOrigins("http://localhost:8111", "http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<StatsHub>("/hubs/stats");
                routes.MapHub<TransactionsHub>("/hubs/tx"); 
                routes.MapHub<PeersHub>("/hubs/peers"); 
                routes.MapHub<NotificationHub>("/hubs/notification");
            });

            //Task.Run(() => smartContractEngine.Run());

            seeder.Init();

            app.UseMvc();
        }
    }
}
