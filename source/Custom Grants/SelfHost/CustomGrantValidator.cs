using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;

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
            var param = request.Raw.Get("some_custom_parameter");
            if (string.IsNullOrWhiteSpace(param))
            {
                return Task.FromResult<CustomGrantValidationResult>(
                    new CustomGrantValidationResult("Missing parameters."));
            }

            return Task.FromResult(new CustomGrantValidationResult("bob", "customGrant"));
        }
    }
}