using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Thinktecture.IdentityServer.v3.AccessTokenValidation;

[assembly: OwinStartup(typeof(Api.Startup))]

namespace Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIdentityServerJwt(new JwtTokenValidationOptions
                {
                    Authority = "https://localhost:44319/identity"
                });

            app.RequireScopes("sampleApi");

            // web api configuration
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            app.UseWebApi(config);
        }
    }
}