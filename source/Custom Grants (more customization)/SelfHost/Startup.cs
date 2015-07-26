using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Host.Config;
using Owin;
using SelfHost.Config;
using SelfHost.Extensions;

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

            factory.ClaimsProvider = 
                new Registration<IClaimsProvider>(typeof(CustomClaimsProvider));
            factory.UserService = 
                new Registration<IUserService>(typeof(CustomUserService));
            factory.CustomGrantValidators.Add( 
                new Registration<ICustomGrantValidator>(typeof(CustomGrantValidator)));

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