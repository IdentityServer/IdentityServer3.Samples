using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
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
