using Owin;
using SelfHost.Config;
using SelfHost.Extensions;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Host.Config;

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
                SiteName = "IdentityServer3 (CustomGrants)",
                RequireSsl = false,

                SigningCertificate = Certificate.Get(),
                Factory = factory,
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}