using EmbeddedMvc.IdentityServer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using System.Linq;

[assembly: OwinStartup(typeof(EmbeddedMvc.Startup))]

namespace EmbeddedMvc
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.Map("/identity", idsrvApp =>
                {
                    idsrvApp.UseIdentityServer(new IdentityServerOptions
                    {
                        SiteName = "Embedded IdentityServer",
                        IssuerUri = "https://idsrv3/embedded",
                        SigningCertificate = LoadCertificate(),

                        Factory = InMemoryFactory.Create(
                            users:   Users.Get(),
                            clients: Clients.Get(),
                            scopes:  Scopes.Get()),

                        AuthenticationOptions = new Thinktecture.IdentityServer.Core.Configuration.AuthenticationOptions
                        {
                            IdentityProviders = ConfigureIdentityProviders
                        }
                    });
                });

            app.UseResourceAuthorization(new AuthorizationManager());

            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                });


            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    Authority = "https://localhost:44319/identity",

                    ClientId = "mvc",
                    Scope = "openid profile roles sampleApi",
                    ResponseType = "code id_token token",
                    RedirectUri = "https://localhost:44319/",

                    SignInAsAuthenticationType = "Cookies",
                    UseTokenLifetime = false,

                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        SecurityTokenValidated = async n =>
                            {
                                var nid = new ClaimsIdentity(
                                    n.AuthenticationTicket.Identity.AuthenticationType,
                                    Constants.ClaimTypes.GivenName,
                                    Constants.ClaimTypes.Role);

                                // get userinfo data
                                var userInfoClient = new UserInfoClient(
                                    new Uri(n.Options.Authority + "/connect/userinfo"),
                                    n.ProtocolMessage.AccessToken);

                                var userInfo = await userInfoClient.GetAsync();
                                userInfo.Claims.ToList().ForEach(ui => nid.AddClaim(new Claim(ui.Item1, ui.Item2)));

                                // keep the id_token for logout
                                nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

                                // add access token for sample API
                                nid.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

                                // keep track of access token expiration
                                nid.AddClaim(new Claim("expires_at", DateTimeOffset.Now.AddSeconds(int.Parse(n.ProtocolMessage.ExpiresIn)).ToString()));

                                // add some other app specific claim
                                nid.AddClaim(new Claim("app_specific", "some data"));

                                n.AuthenticationTicket = new AuthenticationTicket(
                                    nid,
                                    n.AuthenticationTicket.Properties);
                            },
                            RedirectToIdentityProvider = async n =>
                                {
                                    if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                                    {
                                        var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                                        if (idTokenHint != null)
                                        {
                                            n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                                        }
                                    }
                                }
                    }
                });
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
                {
                    AuthenticationType = "Google",
                    Caption = "Sign-in with Google",
                    SignInAsAuthenticationType = signInAsType,

                    ClientId = "701386055558-9epl93fgsjfmdn14frqvaq2r9i44qgaa.apps.googleusercontent.com",
                    ClientSecret = "3pyawKDWaXwsPuRDL7LtKm_o"
                });
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\identityServer\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}