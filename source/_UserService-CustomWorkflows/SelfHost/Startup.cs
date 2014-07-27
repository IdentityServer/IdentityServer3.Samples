using Owin;
using SelfHost.Config;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = InMemoryFactory.Create(
                users:   Users.Get(), 
                clients: Clients.Get(), 
                scopes:  Scopes.Get());

            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer v3 - preview 1 (SelfHost)",
                PublicHostName = "http://localhost:3333",
                SigningCertificate = Certificate.Get(),
                Factory = factory,
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}