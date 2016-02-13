using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Linq;
using System.Security.Claims;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;

[assembly: OwinStartup(typeof(WebApp.Startup))]

namespace WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = "Cookies",
                ExpireTimeSpan = TimeSpan.FromMinutes(10),
                SlidingExpiration = true
            });

            JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = "oidc",
                SignInAsAuthenticationType = "Cookies",
                Authority = "https://localhost:44333/core/",
                ClientId = "webforms.owin.implicit",
                RedirectUri = "http://localhost:5969/",
                PostLogoutRedirectUri = "http://localhost:5969/",
                ResponseType = "id_token token",
                Scope = "openid profile email",
                UseTokenLifetime = false,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async n =>
                    {
                        var claims_to_exclude = new[]
                        {
                            "aud", "iss", "nbf", "exp", "nonce", "iat", "at_hash"
                        };

                        var claims_to_keep =
                            n.AuthenticationTicket.Identity.Claims
                            .Where(x => false == claims_to_exclude.Contains(x.Type)).ToList();
                        claims_to_keep.Add(new Claim("id_token", n.ProtocolMessage.IdToken));

                        if (n.ProtocolMessage.AccessToken != null)
                        {
                            claims_to_keep.Add(new Claim("access_token", n.ProtocolMessage.AccessToken));

                            var userInfoClient = new UserInfoClient(new Uri("https://localhost:44333/core/connect/userinfo"), n.ProtocolMessage.AccessToken);
                            var userInfoResponse = await userInfoClient.GetAsync();
                            var userInfoClaims = userInfoResponse.Claims
                                .Where(x => x.Item1 != "sub") // filter sub since we're already getting it from id_token
                                .Select(x => new Claim(x.Item1, x.Item2));
                            claims_to_keep.AddRange(userInfoClaims);
                        }

                        var ci = new ClaimsIdentity(
                            n.AuthenticationTicket.Identity.AuthenticationType,
                            "name", "role");
                        ci.AddClaims(claims_to_keep);

                        n.AuthenticationTicket = new Microsoft.Owin.Security.AuthenticationTicket(
                            ci, n.AuthenticationTicket.Properties
                        );
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var id_token = n.OwinContext.Authentication.User.FindFirst("id_token")?.Value;
                            n.ProtocolMessage.IdTokenHint = id_token;
                        }
                        
                        return Task.FromResult(0);
                    }
                }
            });
            app.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}
