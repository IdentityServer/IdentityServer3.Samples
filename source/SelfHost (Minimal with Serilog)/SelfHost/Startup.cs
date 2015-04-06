using Owin;
using SelfHost.Config;
using Serilog;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Logging.LogProviders;
using IdentityServer3.Host.Config;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole(
                    outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name}) {NewLine} {Message}{NewLine}{Exception}")
                .CreateLogger();

            LogProvider.SetCurrentLogProvider(new SerilogLogProvider());

            var factory = InMemoryFactory.Create(
                users:   Users.Get(), 
                clients: Clients.Get(), 
                scopes:  Scopes.Get());

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 (self host)",

                SigningCertificate = Certificate.Get(),
                Factory = factory,
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}