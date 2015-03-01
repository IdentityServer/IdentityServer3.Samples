using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Thinktecture.IdentityServer.AccessTokenValidation;

[assembly: OwinStartup(typeof(Apis.Startup))]

namespace Apis
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Disable SSL Validation
            // Credits: http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using
            ServicePointManager.ServerCertificateValidationCallback =
                delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };

            // accept access tokens from identityserver and require a scope of 'api1'
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
                {
                    Authority = "https://localhost:44333",
                    RequiredScopes = new[] { "api1" }
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
