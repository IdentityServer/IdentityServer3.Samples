using Owin;
using SelfHost.Config;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Host.Config;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = InMemoryFactory.Create(
                users : Users.Get(),
                clients: Clients.Get(), 
                scopes:  Scopes.Get());

            //var factory = new IdentityServerServiceFactory();
            factory.TokenSigningService = Registration.RegisterType<ITokenSigningService>(typeof(MyCustomTokenSigningService));
            factory.Register(Registration.RegisterType<ICustomLogger>(typeof(MyCustomDebugLogger)));

            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer v3 - DependencyInjection",
                PublicHostName = "http://localhost:3333",

                SigningCertificate = Certificate.Get(),
                Factory = factory,
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}