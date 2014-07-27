using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;

namespace SelfHost
{
    public class RegistrationUserService : IUserService
    {
        public Task<Thinktecture.IdentityServer.Core.Authentication.ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, Thinktecture.IdentityServer.Core.Models.ExternalIdentity externalUser)
        {
            return Task.FromResult<ExternalAuthenticateResult>(null);
        }

        public Task<Thinktecture.IdentityServer.Core.Authentication.AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string subject, IEnumerable<string> requestedClaimTypes = null)
        {
            return Task.FromResult<IEnumerable<Claim>>(null);
        }

        public Task<bool> IsActive(string subject)
        {
            return Task.FromResult(true);
        }
    }
}
