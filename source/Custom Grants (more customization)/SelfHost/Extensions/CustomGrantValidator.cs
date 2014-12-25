using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

namespace SelfHost.Extensions
{
    class CustomGrantValidator : ICustomGrantValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IUserService _users;

        public CustomGrantValidator(IUserService users)
        {
            _users = users;
        }

        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            if (request.GrantType != "legacy_account_store")
            {
                Logger.Error("unknown custom grant type");
                return null;
            }

            var legacyAccountStoreType = request.Raw.Get("account_store");
            var id = request.Raw.Get("legacy_id");
            var secret = request.Raw.Get("legacy_secret");

            if (legacyAccountStoreType.IsMissing() ||
                id.IsMissing() ||
                secret.IsMissing())
            {
                Logger.Error("malformed request");
                return null;
            }

            var message = new SignInMessage { Tenant = legacyAccountStoreType };
            var result = await _users.AuthenticateLocalAsync(id, secret, message);

            if (result.IsError)
            {
                Logger.Error("authentication failed");
                return new CustomGrantValidationResult("Authentication failed.");
            }

            return new CustomGrantValidationResult(
                result.User.GetSubjectId(),
                "custom",
                result.User.Claims);
        }
    }
}