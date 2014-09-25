using Owin;
using SelfHost.Config;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using Thinktecture.IdentityServer.WsFederation.Services;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = InMemoryFactory.Create(
                users:   Users.Get(), 
                clients: Clients.Get(), 
                scopes:  Scopes.Get());

            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer v3 - beta 1-2 (SelfHost)",
                PublicHostName = "http://localhost:3333",

                SigningCertificate = Certificate.Get(),
                Factory = factory,
                PluginConfiguration = ConfigurePlugins
            };

            appBuilder.UseIdentityServer(options);
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var wsFedOptions = new WsFederationPluginOptions
            {
                IdentityServerOptions = options,
                Factory = new WsFederationServiceFactory
                {
                    UserService = options.Factory.UserService,
                    RelyingPartyService = Registration.RegisterFactory<IRelyingPartyService>(() => new InMemoryRelyingPartyService(RelyingParties.Get())),
                }
            };

            pluginApp.UseWsFederationPlugin(wsFedOptions);
        }
    }
}