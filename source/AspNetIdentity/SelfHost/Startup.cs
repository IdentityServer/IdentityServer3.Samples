using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using SelfHost.Config;
using Thinktecture.IdentityManager;
using Thinktecture.IdentityServer.Core.Configuration;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Map("/admin", adminApp =>
            {
                var factory = new Thinktecture.IdentityManager.Host.AspNetIdentityIdentityManagerFactory("AspId");
                adminApp.UseIdentityManager(new IdentityManagerConfiguration()
                {
                    IdentityManagerFactory = factory.Create
                });
            });

            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer v3 - UserService-AspNetIdentity",
                RequireSsl = false,

                SigningCertificate = Certificate.Get(),
                Factory = Factory.Configure("AspId"),
                CorsPolicy = CorsPolicy.AllowAll,
                AuthenticationOptions = new AuthenticationOptions{
                    IdentityProviders = ConfigureAdditionalIdentityProviders,
                }
            };

            appBuilder.UseIdentityServer(options);
        }

        public static void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "767400843187-8boio83mb57ruogr9af9ut09fkg56b27.apps.googleusercontent.com",
                ClientSecret = "5fWcBT0udKY7_b6E3gEiJlze"
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