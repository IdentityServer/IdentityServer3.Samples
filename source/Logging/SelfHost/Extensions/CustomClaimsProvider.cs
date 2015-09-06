using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SelfHost
{
    class CustomClaimsProvider : DefaultClaimsProvider
    {
        private static readonly ILog Logger = LogProvider.For<CustomClaimsProvider>();

        public CustomClaimsProvider(IUserService users) : base(users)
        { }

        public override Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(System.Security.Claims.ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, ValidatedRequest request)
        {
            Logger.Warn("--- Some custom warning !!!!!!!!!!!!!!!");

            return base.GetIdentityTokenClaimsAsync(subject, client, scopes, includeAllIdentityClaims, request);
        }
    }
}