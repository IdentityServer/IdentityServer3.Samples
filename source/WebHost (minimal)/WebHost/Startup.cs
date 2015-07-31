using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Configuration;
using Thinktecture.IdentityServer.Core.Configuration;

[assembly: OwinStartup(typeof(WebHost.Startup))]

namespace WebHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Map("/core", coreApp =>
            {
                var factory = InMemoryFactory.Create(
                    users: Users.Get(),
                    clients: Clients.Get(),
                    scopes: Scopes.Get());

                var options = new IdentityServerOptions
                {
                    SigningCertificate = Certificate.Load(),
                    Factory = factory,
                };

                coreApp.UseIdentityServer(options);
            });
        }
    }
}