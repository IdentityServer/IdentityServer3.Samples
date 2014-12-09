using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

namespace SelfHost
{
    class CustomGrantValidator : ICustomGrantValidator
    {
        private IUserService _users;
        
        public CustomGrantValidator(IUserService users)
        {
            _users = users;
        }

        Task<CustomGrantValidationResult> ICustomGrantValidator.ValidateAsync(ValidatedTokenRequest request)
        {
            if (request.GrantType != "custom")
            {
                return Task.FromResult<CustomGrantValidationResult>(null);
            }

            var assertion = request.Raw.Get("assertion");
            if (string.IsNullOrWhiteSpace(assertion))
            {
                return Task.FromResult<CustomGrantValidationResult>(new CustomGrantValidationResult
                    {
                        ErrorMessage = "Missing assertion."
                    });
            }

            // validate assertion and return principal
            var principal = IdentityServerPrincipal.Create(
                "bob",
                "bob",
                "custom_grant",
                "idsrv");

            return Task.FromResult(new CustomGrantValidationResult
            {
                Principal = principal
            });
        }
    }
}