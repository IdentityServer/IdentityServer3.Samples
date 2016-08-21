namespace Api
{
    using IdentityServer3.AccessTokenValidation;
    using Microsoft.Owin.Cors;
    using Owin;
    using System.Web.Http;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Allow all origins
            app.UseCors(CorsOptions.AllowAll);

            // Wire token validation
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44300",

                // For access to the introspection endpoint
                ClientId = "api",
                ClientSecret = "api-secret",

                RequiredScopes = new[] { "api" }
            });

            // Wire Web API
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Filters.Add(new AuthorizeAttribute());

            app.UseWebApi(httpConfiguration);
        }
    }
}