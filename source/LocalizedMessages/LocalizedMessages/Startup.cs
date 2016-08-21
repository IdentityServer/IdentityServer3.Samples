using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using SampleApp.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Contrib;
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
                    Factory = factory
                };

                coreApp.UseIdentityServer(options);
            });
        }

    
    }
}