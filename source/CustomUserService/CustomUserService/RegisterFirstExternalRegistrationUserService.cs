using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

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

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
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
            return Task.FromResult<AuthenticateResult>(new AuthenticateResult(user.Subject, name, identityProvider:user.Provider));
        }

        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
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

        public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task SignOutAsync(ClaimsPrincipal subject)
        {
            return Task.FromResult(0);
        }
    }
}
