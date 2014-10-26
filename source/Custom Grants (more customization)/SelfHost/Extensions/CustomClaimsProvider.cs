using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Validation;

namespace SelfHost.Extensions
{
    class CustomClaimsProvider : DefaultClaimsProvider
    {
        public CustomClaimsProvider(IUserService users) : base(users)
        { }

        public override async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ValidatedRequest request)
        {
            var claims = await base.GetAccessTokenClaimsAsync(subject, client, scopes, request);

            var newClaims = claims.ToList();
            newClaims.Add(subject.FindFirst("account_store"));

            return newClaims;
        }
    }
}