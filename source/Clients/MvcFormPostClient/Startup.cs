using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Collections.Generic;
using System.IdentityModel.Tokens;

namespace MvcFormPostClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                });


            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "TempCookie",
                    AuthenticationMode = AuthenticationMode.Passive
                });
        }
    }
}