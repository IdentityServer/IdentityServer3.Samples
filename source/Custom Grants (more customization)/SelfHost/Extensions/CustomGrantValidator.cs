using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using SelfHost.Logging;

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

            if (string.IsNullOrWhiteSpace(legacyAccountStoreType) ||
                string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(secret))
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