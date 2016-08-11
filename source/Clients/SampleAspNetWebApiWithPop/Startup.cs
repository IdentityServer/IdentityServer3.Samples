using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.IdentityModel.Tokens;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin.Security.OAuth;
using IdentityModel.Owin.PopAuthentication;
using Sample;

[assembly: OwinStartup(typeof(SampleAspNetWebApiWithPop.Startup))]

namespace SampleAspNetWebApiWithPop
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                // The HttpSignatureValidation middleware looks for another middleware called PoP
                AuthenticationType = "PoP",

                Authority = Constants.BaseAddress,
                RequiredScopes = new[] { "write" },

                // client credentials for the introspection endpoint
                ClientId = "write",
                ClientSecret = "secret",

                // this is used to extract the access token from the pop token
                TokenProvider = new OAuthBearerAuthenticationProvider
                {
                    OnRequestToken = async ctx =>
                    {
                        ctx.Token = await DefaultPopTokenProvider.GetAccessTokenFromPopTokenAsync(ctx.OwinContext.Environment);
                    }
                }
            });

            // this registers the middleware that does the signature validation of the request against the pop token secret
            app.UseHttpSignatureValidation();

            app.UseWebApi(WebApiConfig.Register());
        }
    }
}
