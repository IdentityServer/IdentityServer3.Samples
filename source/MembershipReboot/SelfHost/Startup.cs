using Owin;
using SelfHost.Config;
using Thinktecture.IdentityServer.Core.Configuration;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer v3 - UserService-MembershipReboot",
                PublicHostName = "http://localhost:3333",
                SigningCertificate = Certificate.Get(),
                Factory = Factory.Configure("MembershipReboot"),
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}