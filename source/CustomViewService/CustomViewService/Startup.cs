using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using SampleApp.Config;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views;
using Thinktecture.IdentityServer.Core.Views.Embedded;

namespace SampleApp
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/core", coreApp =>
            {
                var factory = InMemoryFactory.Create(
                    users : Users.Get(),
                    clients: Clients.Get(),
                    scopes: Scopes.Get());

                factory.ViewService = Registration.RegisterType<IViewService>(typeof(CustomViewService));

                var options = new IdentityServerOptions
                {
                    IssuerUri = "https://idsrv3.com",
                    SiteName = "Thinktecture IdentityServer v3 - CustomViewService",
                    PublicHostName = "http://localhost:3333",
                    SigningCertificate = Certificate.Get(),
                    Factory = factory,
                    AdditionalIdentityProviderConfiguration = ConfigureAdditionalIdentityProviders,
                    CorsPolicy = CorsPolicy.AllowAll,
                };

                coreApp.UseIdentityServer(options);
            });
        }

        public static void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleAuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType
            };
            app.UseGoogleAuthentication(google);

            var fb = new FacebookAuthenticationOptions
            {
                AuthenticationType = "Facebook",
                SignInAsAuthenticationType = signInAsType,
                AppId = "676607329068058",
                AppSecret = "9d6ab75f921942e61fb43a9b1fc25c63"
            };
            app.UseFacebookAuthentication(fb);

            var twitter = new TwitterAuthenticationOptions
            {
                AuthenticationType = "Twitter",
                SignInAsAuthenticationType = signInAsType,
                ConsumerKey = "N8r8w7PIepwtZZwtH066kMlmq",
                ConsumerSecret = "df15L2x6kNI50E4PYcHS0ImBQlcGIt6huET8gQN41VFpUCwNjM"
            };
            app.UseTwitterAuthentication(twitter);
        }
    }
}