using Owin;
using SelfHost.Config;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer v3 - preview 1 (EntityFramework)",

                SigningCertificate = Certificate.Get(),
                Factory = Factory.Configure("IdSvr3Config"),
                CorsPolicy = CorsPolicy.AllowAll
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}