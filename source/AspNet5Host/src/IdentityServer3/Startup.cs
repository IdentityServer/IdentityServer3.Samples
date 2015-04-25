using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Thinktecture.IdentityServer.Core.Configuration;
using AspNet5Host.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace AspNet5Host
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
        }

        public void Configure(IApplicationBuilder app)
        {
            var certFile = AppDomain.CurrentDomain.BaseDirectory + "\\idsrv3test.pfx";

            app.Map("/core", core =>
            {
                var factory = InMemoryFactory.Create(
                                users: Users.Get(),
                                clients: Clients.Get(),
                                scopes: Scopes.Get());

                var idsrvOptions = new IdentityServerOptions
                {
                    Factory = factory,
                    RequireSsl = false,
                    SigningCertificate = new X509Certificate2(certFile, "idsrv3test")
                };

                core.UseIdentityServer(idsrvOptions);
            });
        }
    }
}
