using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Dnx.Runtime;
using Minecraft4Dev.Web.Persistence;
using Minecraft4Dev.Web.Services;
using System.Threading;

namespace AspNetMinecraftLooserboard
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var configurationBuilder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                configurationBuilder.AddUserSecrets();
            }

            configurationBuilder.AddEnvironmentVariables();
            Configuration = configurationBuilder.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Initialize Minecraft configuration and start logs parser
            string minecraftLogsDirectoryPath = Configuration["Minecraft:LogsDirectoryPath"];
            string minecraftJsonDatabaseFilePath = Configuration["Minecraft:JsonDbFilePath"];

            var minecraftLogsDatabase = MinecraftLogsDatabase.LoadFrom(minecraftJsonDatabaseFilePath);
            var minecraftLogsParserService = new MinecraftLogsParserService(minecraftLogsDirectoryPath, minecraftLogsDatabase);
            minecraftLogsParserService.Start(CancellationToken.None);

            // add the database in the dependency configuration
            services.AddInstance<MinecraftLogsDatabase>(minecraftLogsDatabase);

            // Add MVC services to the services container.
            services.AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Configure the HTTP request pipeline.

            // Add the console logger.
            loggerfactory.AddConsole(minLevel: LogLevel.Warning);

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseErrorPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Looserboard}/{action=Index}/{id?}");
            });
        }
    }
}
