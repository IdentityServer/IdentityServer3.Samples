using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;

namespace SampleApp
{
    public class CustomUser
    {
        public string Subject { get; set; }
        public string Provider { get; set; }
        public string ProviderID { get; set; }
        public bool IsRegistered { get; set; }
        public List<Claim> Claims { get; set; }
    }

    public class RegistrationUserService : IUserService
    {
        public static List<CustomUser> Users = new List<CustomUser>();

        public Task<Thinktecture.IdentityServer.Core.Authentication.ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, Thinktecture.IdentityServer.Core.Models.ExternalIdentity externalUser)
        {
            var user = Users.SingleOrDefault(x => x.Provider == externalUser.Provider.Name && x.ProviderID == externalUser.ProviderId);
            string name = "Unknown";
            if (user == null)
            {
                var nameClaim = externalUser.Claims.First(x => x.Type == Constants.ClaimTypes.Name);
                if (nameClaim != null) name = nameClaim.Value;

                user = new CustomUser { 
                    Subject = Guid.NewGuid().ToString(),
                    Provider = externalUser.Provider.Name,
                    ProviderID = externalUser.ProviderId,
                    Claims = new List<Claim> { new Claim(Constants.ClaimTypes.Name, name) }
                };
                Users.Add(user);
            }

            name = user.Claims.First(x => x.Type == Constants.ClaimTypes.Name).Value;

            if (user.IsRegistered)
            {
                return Task.FromResult<ExternalAuthenticateResult>(new ExternalAuthenticateResult(user.Provider, user.Subject, name));
            }
            else
            {
                return Task.FromResult<ExternalAuthenticateResult>(new ExternalAuthenticateResult("/core/register", user.Provider, user.Subject, name));
            }
        }

        public Task<Thinktecture.IdentityServer.Core.Authentication.AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string subject, IEnumerable<string> requestedClaimTypes = null)
        {
            var user = Users.SingleOrDefault(x => x.Subject == subject);
            if (user == null)
            {
                return Task.FromResult<IEnumerable<Claim>>(null);
            }

            return Task.FromResult(user.Claims.Where(x => requestedClaimTypes.Contains(x.Type)));
        }

        public Task<bool> IsActive(string subject)
        {
            return Task.FromResult(true);
        }
    }
}
