using IdentityServer.Host.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Owin;
using SelfHost.Config;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = new IdentityServerServiceFactory()
                               .UseInMemoryUsers(Users.Get())
                               .UseInMemoryScopes(Scopes.Get())
                               .UseInMemoryClients(Clients.Get());

            factory.CustomGrantValidators.Add( 
                new Registration<ICustomGrantValidator>(typeof(CustomGrantValidator)));

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