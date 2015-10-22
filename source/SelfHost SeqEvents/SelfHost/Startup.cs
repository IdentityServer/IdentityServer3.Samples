using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Owin;
using SelfHost.Config;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = new IdentityServerServiceFactory()
                .UseInMemoryClients(Clients.Get())
                .UseInMemoryScopes(Scopes.Get())
                .UseInMemoryUsers(Users.Get());

            factory.EventService = new Registration<IEventService, SeqEventService>();

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 (self host)",

                SigningCertificate = Certificate.Get(),
                Factory = factory,

                EventsOptions = new EventsOptions
                {
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true,
                    RaiseSuccessEvents = true
                }
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}