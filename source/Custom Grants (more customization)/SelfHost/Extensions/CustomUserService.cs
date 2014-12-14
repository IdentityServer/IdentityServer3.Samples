using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace SelfHost.Extensions
{
    class CustomUserService : IUserService
    {
        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message = null)
        {
            if (message != null)
            {
                var tenant = message.Tenant;

                if (username == password)
                {
                    var user = IdentityServerPrincipal.Create("123", username);
                    var claims = new List<Claim>
                    {
                        new Claim("account_store", tenant)
                    };

                    return Task.FromResult(new AuthenticateResult("123", username, claims));
                }
            }

            // default account store
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync(ClaimsPrincipal subject, IDictionary<string, object> env)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync(ClaimsPrincipal subject)
        {
            throw new NotImplementedException();
        }
    }
}