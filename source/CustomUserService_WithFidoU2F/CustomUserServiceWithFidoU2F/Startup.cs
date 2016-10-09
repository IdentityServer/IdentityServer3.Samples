using Owin;
using SampleApp.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Serilog;
using IdentityServer3.Host.Config;
using SampleApp.Repositories;
using SampleApp.Services;

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
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());

                // different examples of custom user services
                var userRepository = new InMemoryUserRepository();
                
                var userService = new U2FLoginUserService(userRepository);

                // note: for the sample this registration is a singletone (not what you want in production probably)
                factory.UserService = new Registration<IUserService>(resolver => userService);

                var options = new IdentityServerOptions
                {
                    SiteName = "IdentityServer3 - Custom User Service with Fido U2F",

                    SigningCertificate = Certificate.Get(),
                    Factory = factory,
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