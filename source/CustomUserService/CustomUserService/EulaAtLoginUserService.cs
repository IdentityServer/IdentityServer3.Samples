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
using IdentityServer3.Core.Services.Default;

namespace SampleApp
{
    public class EulaAtLoginUserService : UserServiceBase
    {
        public class CustomUser
        {
            public string Subject { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool AcceptedEula { get; set; }
            public List<Claim> Claims { get; set; }
        }

        public static List<CustomUser> Users = new List<CustomUser>()
        {
            new CustomUser{
                Subject = "123", 
                Username = "alice", 
                Password = "alice", 
                AcceptedEula = false, 
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
                AcceptedEula = false, 
                Claims = new List<Claim>{
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                    new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                }
            },
        };

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var user = Users.SingleOrDefault(x => x.Username == context.UserName && x.Password == context.Password);
            if (user != null)
            {
                if (user.AcceptedEula)
                {
                    context.AuthenticateResult = new AuthenticateResult(user.Subject, user.Username);
                }
                else
                {
                    context.AuthenticateResult = new AuthenticateResult("~/eula", user.Subject, user.Username);
                }
            }

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // issue the claims for the user
            var user = Users.SingleOrDefault(x => x.Subject == context.Subject.GetSubjectId());
            if (user != null)
            {
                context.IssuedClaims = user.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            }

            return Task.FromResult(0);
        }
    }
}
