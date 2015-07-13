using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Sample;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
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
                    ClientId = "katanaclient",
                    Authority = Constants.BaseAddress,
                    RedirectUri = "http://localhost:2672/",
                    PostLogoutRedirectUri = "http://localhost:2672/",
                    ResponseType = "code id_token token",
                    Scope = "openid email profile read write offline_access",

                    SignInAsAuthenticationType = "Cookies",

                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthorizationCodeReceived = async n =>
                            {
                                // get userinfo data
                                var userInfoClient = new UserInfoClient(
                                    new Uri(Constants.UserInfoEndpoint),
                                    n.ProtocolMessage.AccessToken);

                                var userInfo = await userInfoClient.GetAsync();
                                var id = userInfo.GetClaimsIdentity();
                                
                                // get access and refresh token
                                var tokenClient = new TokenClient(
                                    Constants.TokenEndpoint,
                                    "katanaclient",
                                    "secret");

                                var response = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);

                                id.AddClaim(new Claim("access_token", response.AccessToken));
                                id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(response.ExpiresIn).ToLocalTime().ToString()));
                                id.AddClaim(new Claim("refresh_token", response.RefreshToken));
                                id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

                                n.AuthenticationTicket = new AuthenticationTicket(
                                    new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType), 
                                    n.AuthenticationTicket.Properties);
                            },

                        RedirectToIdentityProvider = n =>
                            {
                                // if signing out, add the id_token_hint
                                if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                                {
                                    var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                                    if (idTokenHint != null)
                                    {
                                        n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                                    }

                                }

                                return Task.FromResult(0);
                            }
                    }
                });
        }
    }
}