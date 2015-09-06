using Microsoft.Owin;
using Owin;
using System.Web.Http;
using IdentityServer3.AccessTokenValidation;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

[assembly: OwinStartup(typeof(Api.Startup))]

namespace Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // token validation
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44319/identity",
                RequiredScopes = new[] { "sampleApi" }
            });

            // add app local claims per request
            app.UseClaimsTransformation(incoming =>
            {
                // either add claims to incoming, or create new principal
                var appPrincipal = new ClaimsPrincipal(incoming);
                incoming.Identities.First().AddClaim(new Claim("appSpecific", "some_value"));

                return Task.FromResult(appPrincipal);
            });

            // web api configuration
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            app.UseWebApi(config);
        }
    }
}