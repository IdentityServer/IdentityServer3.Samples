using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer
{
    class ActAsGrantValidator : ICustomGrantValidator
    {
        private TokenValidator _validator;

        public ActAsGrantValidator(TokenValidator validator)
        {
            _validator = validator;
        }

        Task<CustomGrantValidationResult> ICustomGrantValidator.ValidateAsync(ValidatedTokenRequest request)
        {
            var param = request.Raw.Get("token");
            if (string.IsNullOrWhiteSpace(param))
            {
                return Task.FromResult<CustomGrantValidationResult>(
                    new CustomGrantValidationResult("Token parameter not set."));
            }

            var result = _validator.ValidateAccessTokenAsync(param).Result;
            if (result.IsError)
            {
                return Task.FromResult<CustomGrantValidationResult>(
                    new CustomGrantValidationResult(result.Error));
            }

            var subjectClaim = result.Claims.FirstOrDefault(x => x.Type == "sub");
            if(subjectClaim == null)
            {
                return Task.FromResult<CustomGrantValidationResult>(
                    new CustomGrantValidationResult("No subject claim for the token."));
            }

            return Task.FromResult(new CustomGrantValidationResult(subjectClaim.Value, "act-as"));
        }

        public string GrantType
        {
            get { return "act-as"; }
        }
    }
}
