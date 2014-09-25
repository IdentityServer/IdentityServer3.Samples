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
    public class EulaAtLoginUserService : IUserService
    {
        public class CustomUser
        {
            public string Subject { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool AccpetedEula { get; set; }
            public List<Claim> Claims { get; set; }
        }

        public static List<CustomUser> Users = new List<CustomUser>()
        {
            new CustomUser{
                Subject = "123", 
                Username = "alice", 
                Password = "alice", 
                AccpetedEula = false, 
                Claims = new List<Claim>{
                    new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                    new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                }
            },
            new CustomUser{
                Subject = "890", 
                Username = "bob", 
                Password = "bob", 
                AccpetedEula = false, 
                Claims = new List<Claim>{
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                    new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                }
            },
        };

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
            if (user.AccpetedEula)
            {
                return Task.FromResult<AuthenticateResult>(new AuthenticateResult(user.Subject, user.Username));
            }
            else
            {
                return Task.FromResult<AuthenticateResult>(new AuthenticateResult("/core/eula", user.Subject, user.Username));
            }
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
            return Task.FromResult(user != null && user.AccpetedEula);
        }
    }
}
