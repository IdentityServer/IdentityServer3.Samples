using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using Microsoft.Owin;

namespace SampleApp.Extensions
{
    public static class OwinEnvironmentExtensions
    {
        public static async Task UpdateAuthenticationMethodForU2FAsync(this IDictionary<string, object> env)
        {
            var ctx = new OwinContext(env);

            var authentication =
                await ctx.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);

            await
                env.UpdatePartialLoginClaimsAsync(authentication.Identity.GetSubjectId(), authentication.Identity.Name,
                    authentication.Identity.Claims, Constants.BuiltInIdentityProvider,
                    Constants.AuthenticationMethods.TwoFactorAuthentication);
        }
    }
}