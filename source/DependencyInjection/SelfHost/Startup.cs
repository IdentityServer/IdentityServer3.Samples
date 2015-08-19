using Owin;
using SelfHost.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Host.Config;
using IdentityServer3.Core.Services.Default;
using Serilog;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Trace()
               .CreateLogger();

            var factory = new IdentityServerServiceFactory()
                .UseInMemoryUsers(Users.Get())
                .UseInMemoryClients(Clients.Get()) 
                .UseInMemoryScopes(Scopes.Get());

            factory.ClaimsProvider = new Registration<IClaimsProvider, MyCustomClaimsProvider>();
            factory.Register(new Registration<ICustomLogger, MyCustomDebugLogger>());
            factory.CorsPolicyService = new Registration<ICorsPolicyService>(new DefaultCorsPolicyService { AllowAll = true });

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 - DependencyInjection",

                SigningCertificate = Certificate.Get(),
                Factory = factory,
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}