using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Sample;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(MVC_OWIN_Client.Startup))]

namespace MVC_OWIN_Client
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

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = "implicitclient",
                Authority = Constants.BaseAddress,
                RedirectUri = "https://localhost:44301/",
                ResponseType = "id_token token",
                Scope = "openid email write",

                UseTokenLifetime = false,
                SignInAsAuthenticationType = "Cookies",

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async n =>
                    {
                        // simulate a slow authorization response page
                        //await Task.Delay(2000);

                        var token = n.ProtocolMessage.AccessToken;

                        // persist access token in cookie
                        if (!string.IsNullOrEmpty(token))
                        {
                            n.AuthenticationTicket.Identity.AddClaim(
                                new Claim("access_token", token));
                        }

                        //return Task.FromResult(0);
                    }
                }
            });
        }
    }
}