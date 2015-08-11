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
    public class RegisterFirstExternalRegistrationUserService : UserServiceBase
    {
        public class CustomUser
        {
            public string Subject { get; set; }
            public string Provider { get; set; }
            public string ProviderID { get; set; }
            public List<Claim> Claims { get; set; }
        }
        
        public static List<CustomUser> Users = new List<CustomUser>();

        public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            var externalUser = context.ExternalIdentity;

            // look for the user in our local identity system from the external identifiers
            var user = Users.SingleOrDefault(x => x.Provider == externalUser.Provider && x.ProviderID == externalUser.ProviderId);
            if (user == null)
            {
                // user is not registered so redirect
                context.AuthenticateResult = new AuthenticateResult("~/registerfirstexternalregistration", externalUser);
            }
            else
            {
                // user is registered so continue
                var name = user.Claims.First(x => x.Type == Constants.ClaimTypes.Name).Value;
                context.AuthenticateResult = new AuthenticateResult(user.Subject, name, identityProvider: user.Provider);
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
