using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer3.Core.Configuration;
using Microsoft.Framework.Runtime;

namespace IdentityServerAspNet5
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
        }

        public void Configure(IApplicationBuilder app, IApplicationEnvironment env)
        {
            var certFile = env.ApplicationBasePath + "\\idsrv3test.pfx";

            var factory = new IdentityServerServiceFactory()
                               .UseInMemoryUsers(Users.Get())
                               .UseInMemoryClients(Clients.Get())
                               .UseInMemoryScopes(Scopes.Get());

            var idsrvOptions = new IdentityServerOptions
            {
                Factory = factory,
                RequireSsl = false,
                SigningCertificate = new X509Certificate2(certFile, "idsrv3test")
            };

            app.UseIdentityServer(idsrvOptions);
        }
    }
}
