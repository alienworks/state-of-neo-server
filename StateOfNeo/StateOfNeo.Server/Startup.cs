using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;

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
            Configuration = builder.Build();

            //this.StartBlockchain();
        }

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

            services.AddDbContext<StateOfNeoContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
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
            else
            {
                app.UseHsts();
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

            //app.UseHttpsRedirection();
            seeder.Init();

            notificationEngine.Init();

            app.UseMvc();
        }

    }
}
