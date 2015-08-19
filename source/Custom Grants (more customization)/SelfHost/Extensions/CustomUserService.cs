using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SelfHost.Extensions
{
    class CustomUserService : UserServiceBase
    {
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var username = context.UserName;
            var password = context.Password;
            var message = context.SignInMessage;

            if (message != null)
            {
                var tenant = message.Tenant;

                if (username == password)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("account_store", tenant)
                    };

                    var result = new AuthenticateResult("123", username,
                        claims: claims,
                        authenticationMethod: "custom");

                    context.AuthenticateResult = new AuthenticateResult("123", username, claims);
                }
            }

            return Task.FromResult(0);
        }
    }
}