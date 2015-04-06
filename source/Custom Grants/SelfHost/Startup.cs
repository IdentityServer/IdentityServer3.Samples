using Owin;
using SelfHost.Config;
using IdentityServer.Host.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;

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

            factory.CustomGrantValidator = 
                new Registration<ICustomGrantValidator>(typeof(CustomGrantValidator));

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 (CustomGrants)",

                SigningCertificate = Certificate.Get(),
                Factory = factory,
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}