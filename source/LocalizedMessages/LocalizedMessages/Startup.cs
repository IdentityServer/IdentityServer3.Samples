using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using SampleApp.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Contrib;
using IdentityServer3.Core.Services.Default;
using Serilog;
using IdentityServer3.Host.Config;
using Microsoft.Owin;

namespace SampleApp
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                           .MinimumLevel.Debug()
                           .WriteTo.Trace()
                           .CreateLogger();

            app.Map("/core", coreApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryUsers(Users.Get())
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());


                var opts = new LocaleOptions
                {
                    LocaleProvider = env =>
                    {
                        var owinContext = new OwinContext(env);
                        var owinRequest = owinContext.Request;
                        var headers = owinRequest.Headers;
                        var accept_language_header = headers["accept-language"].ToString();
                        var languages = accept_language_header.Split(',').Select(StringWithQualityHeaderValue.Parse).OrderByDescending(s => s.Quality.GetValueOrDefault(1));
                        var locale = languages.First().Value;

                        return locale;
                    }
                };
                factory.Register(new Registration<LocaleOptions>(opts));
                factory.LocalizationService = new Registration<ILocalizationService, GlobalizedLocalizationService>();
                

                var options = new IdentityServerOptions
                {
                    SiteName = "IdentityServer33 - Localized from accept-language http header Messages",

                    SigningCertificate = Certificate.Get(),
                    Factory = factory,
                    AuthenticationOptions = new AuthenticationOptions {
                        IdentityProviders = ConfigureAdditionalIdentityProviders,
                    }
                };

                coreApp.UseIdentityServer(options);
            });
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