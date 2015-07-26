using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using IdentityServer3.AccessTokenValidation;

[assembly: OwinStartup(typeof(SampleAspNetWebApi.Startup))]

namespace SampleAspNetWebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
                {
                    Authority = "https://localhost:44333/core",
                    RequiredScopes = new[] { "write" }
                });

            app.UseWebApi(WebApiConfig.Register());
        }
    }
}