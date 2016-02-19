using Owin;
using SampleApp.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Serilog;
using Common;

namespace CustomLoginPage
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File(@"c:\logs\idsvr3.samples.CustomLoginPage.txt")
               .CreateLogger();

            app.Map("/core", coreApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());

                factory.UserService = new Registration<IUserService, CustomLoginPageUserService>();

                var options = new IdentityServerOptions
                {
                    SiteName = "IdentityServer3 - Custom Login Page",

                    SigningCertificate = Certificate.Get(),
                    Factory = factory,
                    
                    AuthenticationOptions = new AuthenticationOptions
                    {
                    },

                    EventsOptions = new EventsOptions
                    {
                        RaiseSuccessEvents = true,
                        RaiseErrorEvents = true,
                        RaiseFailureEvents = true,
                        RaiseInformationEvents = true
                    }
                };

                coreApp.UseIdentityServer(options);
            });
        }
    }
}