using Owin;
using SelfHost.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 - (EntityFramework)",
                SigningCertificate = Certificate.Get(),
                Factory = Factory.Configure("IdSvr3Config"),
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}