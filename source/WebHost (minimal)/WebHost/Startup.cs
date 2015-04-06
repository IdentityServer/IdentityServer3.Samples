using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Configuration;
using IdentityServer3.Core.Configuration;

[assembly: OwinStartup(typeof(WebHost.Startup))]

namespace WebHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
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

            appBuilder.UseIdentityServer(options);
        }
    }
}