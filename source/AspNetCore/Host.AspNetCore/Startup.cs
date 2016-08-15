using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Builder;
using Owin;
using Microsoft.AspNetCore.DataProtection;

namespace Host.AspNetCore
{
    using IdentityServer3.Core.Configuration;
    using IdentityServer3.Host.Config;
    using Microsoft.AspNetCore.Hosting;
    using SampleApp.Config;
    using Serilog;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment host)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File(@"c:\logs\idsvrlog.txt")
               .CreateLogger();

            app.Map("/core", coreApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get())
                    .UseInMemoryUsers(Users.Get());

                var options = new IdentityServerOptions
                {
                    SiteName = "IdentityServer3 - AspNet Core",

                    SigningCertificate = Certificate.Get(host.ContentRootPath),
                    Factory = factory,
                };

                coreApp.UseIdentityServer(options);
            });
        }
    }
}
