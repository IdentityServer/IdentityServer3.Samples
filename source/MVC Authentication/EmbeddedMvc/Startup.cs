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
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;

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
                    Scope = "openid profile roles",
                    RedirectUri = "https://localhost:44319/",

                    SignInAsAuthenticationType = "Cookies",
                    UseTokenLifetime = false,

                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        SecurityTokenValidated = async n =>
                            {
                                var id = n.AuthenticationTicket.Identity;

                                // we want to keep first name, last name, subject and roles
                                var givenName = id.FindFirst(Constants.ClaimTypes.GivenName);
                                var familyName = id.FindFirst(Constants.ClaimTypes.FamilyName);
                                var sub = id.FindFirst(Constants.ClaimTypes.Subject);
                                var roles = id.FindAll(Constants.ClaimTypes.Role);

                                // create new identity and set name and role claim type
                                var nid = new ClaimsIdentity(
                                    id.AuthenticationType,
                                    Constants.ClaimTypes.GivenName,
                                    Constants.ClaimTypes.Role);

                                nid.AddClaim(givenName);
                                nid.AddClaim(familyName);
                                nid.AddClaim(sub);
                                nid.AddClaims(roles);

                                // keep the id_token for logout
                                nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

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