using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using IdentityServer3.AccessTokenValidation;
using System.Web.Http;

[assembly: OwinStartup(typeof(WebApi2.Startup))]

namespace WebApi2
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "http://localhost:44333",
                ValidationMode = ValidationMode.ValidationEndpoint,
                RequiredScopes = new[] { "WebApi2" }
            });

            // configure web api
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            // require authentication for all controllers
            config.Filters.Add(new AuthorizeAttribute());

            app.UseWebApi(config);
        }
    }
}
