using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

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
                "idsrv3");

            return Task.FromResult(principal);
        }
    }
}