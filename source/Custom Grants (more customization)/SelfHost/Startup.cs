using Owin;
using SelfHost.Config;
using SelfHost.Extensions;
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
                users:   Users.Get(), 
                clients: Clients.Get(), 
                scopes:  Scopes.Get());

            factory.ClaimsProvider = 
                new Registration<IClaimsProvider>(typeof(CustomClaimsProvider));
            factory.UserService = 
                new Registration<IUserService>(typeof(CustomUserService));
            factory.CustomGrantValidator = 
                new Registration<ICustomGrantValidator>(typeof(CustomGrantValidator));

            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer3 (CustomGrants)",
                RequireSsl = false,

                SigningCertificate = Certificate.Get(),
                Factory = factory,
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}