using System;
using System.Threading.Tasks;
using System.Web;
using EnhancedCoding.Samples.IdSvrServices;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using MvcViewServiceSample.Controllers;
using Owin;
using Configuration;
using IdentityServer3.Core.Configuration;
using Serilog;
using Common;

[assembly: OwinStartup(typeof(MvcViewServiceSample.Startup))]

namespace MvcViewServiceSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace(outputTemplate: "{Timestamp} [{Level}] ({Name}){NewLine} {Message}{NewLine}{Exception}")
                .CreateLogger();

            var factory = new IdentityServerServiceFactory()
                        .UseInMemoryUsers(Users.Get())
                        .UseInMemoryClients(Clients.Get())
                        .UseInMemoryScopes(Scopes.Get());

            // Use the Mvc View Service instead of the default
            factory.ViewService = new Registration<IViewService, MvcViewService<LogonWorkflowController>>();

            // These registrations are also needed since these are dealt with using non-standard construction
            factory.Register(new Registration<HttpContext>(resolver => HttpContext.Current));
            factory.Register(new Registration<HttpContextBase>(resolver => new HttpContextWrapper(resolver.Resolve<HttpContext>())));
            factory.Register(new Registration<HttpRequestBase>(resolver => resolver.Resolve<HttpContextBase>().Request));
            factory.Register(new Registration<HttpResponseBase>(resolver => resolver.Resolve<HttpContextBase>().Response));
            factory.Register(new Registration<HttpServerUtilityBase>(resolver => resolver.Resolve<HttpContextBase>().Server));
            factory.Register(new Registration<HttpSessionStateBase>(resolver => resolver.Resolve<HttpContextBase>().Session));

            var options = new IdentityServerOptions
            {
                SigningCertificate = Certificate.Load(),
                Factory = factory
            };

            appBuilder.Map("/core", idsrvApp =>
                {
                    idsrvApp.UseIdentityServer(options);
                });
        }
    }
}