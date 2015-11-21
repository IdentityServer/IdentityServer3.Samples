using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer3.Core.Configuration;
using IdentityServer.Configuration;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Serilog;

namespace IdentityServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
        }

        public void Configure(IApplicationBuilder app, IApplicationEnvironment env, ILoggerFactory loggerFactory)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.UseIISPlatformHandler();
            

            var certFile = env.ApplicationBasePath + "\\idsrv3test.pfx";

            var idsrvOptions = new IdentityServerOptions
            {
                Factory = new IdentityServerServiceFactory()
                                .UseInMemoryUsers(Users.Get())
                                .UseInMemoryClients(Clients.Get())
                                .UseInMemoryScopes(Scopes.Get()),

                SigningCertificate = new X509Certificate2(certFile, "idsrv3test"),
                RequireSsl = false
            };

            app.UseIdentityServer(idsrvOptions);
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}