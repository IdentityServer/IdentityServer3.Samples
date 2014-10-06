using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(MvcCodeFlowClientManual.Startup))]

namespace MvcCodeFlowClientManual
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;

            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                });
        }
    }
}
