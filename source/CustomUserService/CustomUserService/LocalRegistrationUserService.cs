using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace SampleApp
{
    public class LocalRegistrationUserService : IUserService
    {
        public class CustomUser
        {
            public string Subject { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public List<Claim> Claims { get; set; }
        }
        
        public static List<CustomUser> Users = new List<CustomUser>();

        public Task<ExternalAuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser)
        {
            return Task.FromResult<ExternalAuthenticateResult>(null);
        }

        public Task<Thinktecture.IdentityServer.Core.Authentication.AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            var user = Users.SingleOrDefault(x => x.Username == username && x.Password == password);
            if (user == null)
            {
                return Task.FromResult<AuthenticateResult>(null);
            }
            
            return Task.FromResult<AuthenticateResult>(new AuthenticateResult(user.Subject, user.Username));
        }

        public Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string subject, IEnumerable<string> requestedClaimTypes = null)
        {
            // issue the claims for the user
            var user = Users.SingleOrDefault(x => x.Subject == subject);
            if (user == null)
            {
                return Task.FromResult<IEnumerable<Claim>>(null);
            }

            return Task.FromResult(user.Claims.Where(x => requestedClaimTypes.Contains(x.Type)));
        }

        public Task<bool> IsActive(string subject)
        {
            var user = Users.SingleOrDefault(x => x.Subject == subject);
            return Task.FromResult(user != null);
        }
    }
}
