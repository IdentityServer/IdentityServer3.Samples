using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

namespace SelfHost
{
    class CustomGrantValidator : ICustomGrantValidator
    {
        private IUserService _users;
        
        public CustomGrantValidator(IUserService users)
        {
            _users = users;
        }

        public Task<ClaimsPrincipal> ValidateAsync(ValidatedTokenRequest request)
        {
            if (request.GrantType != "custom")
            {
                return Task.FromResult<ClaimsPrincipal>(null);
            }

            var assertion = request.Raw.Get("assertion");
            if (string.IsNullOrWhiteSpace(assertion))
            {
                return Task.FromResult<ClaimsPrincipal>(null);
            }

            // validate assertion and return principal
            var principal = IdentityServerPrincipal.Create(
                "bob",
                "bob",
                "custom_grant",
                "idsrv");

            return Task.FromResult(principal);
        }
    }
}