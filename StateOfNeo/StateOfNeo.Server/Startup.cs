using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo;
using Neo.Network.P2P;
using Neo.Persistence.LevelDB;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Seed;
using StateOfNeo.Infrastructure.Mapping;
using StateOfNeo.Server.Actors;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
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
            AutoMapperConfig.Init();

            services.Configure<NetSettings>(this.Configuration.GetSection("NetSettings"));

            services.AddScoped<NodeCache>();
            services.AddScoped<NodeSynchronizer>();
            services.AddScoped<RPCNodeCaller>();
            services.AddScoped<PeersEngine>();
            services.AddScoped<LocationCaller>();

            services
                .AddDbContext<StateOfNeoContext>(options => options.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")))
                .AddEntityFrameworkSqlServer();

            services.AddTransient<StateOfNeoSeedData>();
            services.AddTransient<NotificationEngine>();

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
            NotificationEngine notificationEngine,
            IHubContext<BlockHub> blockHub)
        {
            Program.NeoSystem.ActorSystem.ActorOf(BlockPersister.Props(Program.NeoSystem.Blockchain, this.Configuration.GetConnectionString("DefaultConnection"), blockHub));
            
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

    }
}
