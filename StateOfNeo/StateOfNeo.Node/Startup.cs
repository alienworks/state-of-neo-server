using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Persistence.LevelDB;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Seed;
using StateOfNeo.Infrastructure.Mapping;
using StateOfNeo.Node.Cache;
using StateOfNeo.Node.Common;
using StateOfNeo.Node.Hubs;
using StateOfNeo.Node.Infrastructure;

namespace StateOfNeo.Node
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            InitializeNeoSystem();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public static NeoSystem NeoSystem { get; private set; }
        private Settings Settings = new Settings();

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Automapper configuration initialization
            AutoMapperConfig.Init();

            services.Configure<NetSettings>(Configuration.GetSection("NetSettings"));

            services.AddScoped<NodeCache>();
            services.AddScoped<NodeSynchronizer>();
            services.AddScoped<RPCNodeCaller>();
            services.AddScoped<PeersEngine>();
            services.AddScoped<LocationCaller>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<StateOfNeoContext>(options =>
            {
                options.UseSqlServer(connectionString);
            })
            .AddEntityFrameworkSqlServer();

            services.AddTransient<StateOfNeoSeedData>();

            services.AddTransient<NotificationEngine>();

            services.AddCors();
            services.AddSignalR();
            services.AddMvc(options =>
            {
                //options.SslPort = 5001;
                //options.Filters.Add(new RequireHttpsAttribute());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            StateOfNeoSeedData seeder,
            NotificationEngine notificationEngine)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Shows UseCors with CorsPolicyBuilder.
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
                routes.MapHub<BlockHub>("/hubs/block");
                routes.MapHub<NodeHub>("/hubs/node");
                routes.MapHub<TransactionCountHub>("/hubs/trans-count");
                routes.MapHub<TransactionAverageCountHub>("/hubs/trans-average-count");
                routes.MapHub<FailedP2PHub>("/hubs/fail-p2p");
            });
            
            seeder.Init();

            notificationEngine.Init();

            app.UseMvc();
        }

        private void InitializeNeoSystem()
        {
            LevelDBStore store = new LevelDBStore(Settings.Paths.Chain);
            NeoSystem = new NeoSystem(store);

            NeoSystem.ActorSystem.ActorOf(NotificationBroadcaster.Props(NeoSystem.Blockchain));
            NeoSystem.StartNode(Settings.P2P.Port, Settings.P2P.WsPort);
        }
    }
}
