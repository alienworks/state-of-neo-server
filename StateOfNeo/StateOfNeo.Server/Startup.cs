using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Network.P2P;
using Neo.Persistence.LevelDB;
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

            services.AddSingleton<IMainStatsState, MainStatsState>();
            services.AddSingleton<IStateService, StateService>();
        //    services.AddSingleton(new BalanceUpdater(this.Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value));

            // Infrastructure
            services.AddScoped<NodeCache>();
            services.AddScoped<NodeSynchronizer>();
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
            StateOfNeoSeedData seeder,
            StateOfNeoContext ctx,
            IServiceProvider services,
            IOptions<NetSettings> netSettings,
            IOptions<ImportBlocksSettings> importSettings,
            IHubContext<StatsHub> statsHub,
            IHubContext<NotificationHub> notificationHub, 
            RPCNodeCaller nodeCaller,
            IStateService state)
        {
            var connectionString = this.Configuration.GetConnectionString("DefaultConnection");
            Program.NeoSystem.ActorSystem.ActorOf(BlockPersister.Props(
                Program.NeoSystem.Blockchain,
                connectionString,
                state,
                statsHub,
                notificationHub,
                netSettings.Value.Net));

            //new ImportBlocks(importSettings.Value.MaxOnImportHeight);

            Program.NeoSystem.ActorSystem.ActorOf(NodePersister.Props(
                Program.NeoSystem.Blockchain,
                connectionString,
                netSettings.Value.Net,
                nodeCaller));

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
                    .WithOrigins("http://stateofneo.io", "https://stateofneo.io", "http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<StatsHub>("/hubs/stats");
                routes.MapHub<NodeHub>("/hubs/node");
                routes.MapHub<NotificationHub>("/hubs/notification");
            });

            seeder.Init();

            app.UseMvc();
        }
    }
}
