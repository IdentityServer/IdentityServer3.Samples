using EmbeddedMvc.IdentityServer;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

[assembly: OwinStartup(typeof(EmbeddedMvc.Startup))]

namespace EmbeddedMvc
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());

            app.Map("/identity", idsrvApp =>
                {
                    idsrvApp.UseIdentityServer(new IdentityServerOptions
                    {
                        SiteName = "Embedded IdentityServer",
                        IssuerUri = "https://idsrv3/embedded",
                        SigningCertificate = LoadCertificate(),

                        Factory = InMemoryFactory.Create(
                            users  : Users.Get(),
                            clients: Clients.Get(),
                            scopes : Scopes.Get())
                    });
                });

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

                    SignInAsAuthenticationType = "Cookies"
                });
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\identityServer\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}