using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Plumbing;

namespace SampleApp
{
    public class RegisterFirstExternalRegistrationUserService : IUserService
    {
        public class CustomUser
        {
            public string Subject { get; set; }
            public string Provider { get; set; }
            public string ProviderID { get; set; }
            public List<Claim> Claims { get; set; }
        }
        
        public static List<CustomUser> Users = new List<CustomUser>();

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser)
        {
            // look for the user in our local identity system from the external identifiers
            var user = Users.SingleOrDefault(x => x.Provider == externalUser.Provider && x.ProviderID == externalUser.ProviderId);
            if (user == null)
            {
                // user is not registered so redirect
                return Task.FromResult<AuthenticateResult>(new AuthenticateResult("/core/registerfirstexternalregistration", externalUser));
            }

            // user is registered so continue
            var name = user.Claims.First(x => x.Type == Constants.ClaimTypes.Name).Value;
            var p = IdentityServerPrincipal.Create(
                user.Subject, name,
                Thinktecture.IdentityServer.Core.Constants.AuthenticationMethods.External,
                user.Provider
            );
            return Task.FromResult<AuthenticateResult>(new AuthenticateResult(p));
        }

        public Task<Thinktecture.IdentityServer.Core.Authentication.AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            // issue the claims for the user
            var user = Users.SingleOrDefault(x => x.Subject == subject.GetSubjectId());
            if (user == null)
            {
                return Task.FromResult<IEnumerable<Claim>>(null);
            }

            return Task.FromResult(user.Claims.Where(x => requestedClaimTypes.Contains(x.Type)));
        }

        public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            var user = Users.SingleOrDefault(x => x.Subject == subject.GetSubjectId());
            return Task.FromResult(user != null);
        }

        public Task<AuthenticateResult> PreAuthenticateAsync(IDictionary<string, object> env, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }
    }
}
