using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace SampleWCFApiHost.CustomToken
{
    /// <summary>
    /// CustomTokenAuthenticator for use with the AccessToken
    /// This validates the incoming token against a CustomJwtSecurityTokenHandler validator
    /// </summary>
    class CustomTokenAuthenticator : SecurityTokenAuthenticator
    {
        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is CustomToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            CustomToken CustomToken = token as CustomToken;

            CustomJwtSecurityTokenHandler.CustomJwtSecurityTokenHandler customJwtSecurityTokenHandler = new CustomJwtSecurityTokenHandler.CustomJwtSecurityTokenHandler();
            
            ReadOnlyCollection<System.Security.Claims.ClaimsIdentity> auth = customJwtSecurityTokenHandler.ValidateToken(token);                       

            List<System.IdentityModel.Claims.Claim> claimList = new List<System.IdentityModel.Claims.Claim>();

            foreach(System.Security.Claims.ClaimsIdentity claimsIdentity in auth)
            {
                foreach (var claim in claimsIdentity.Claims)
                {
                    claimList.Add(new System.IdentityModel.Claims.Claim(claim.Type, claim.Value, Rights.PossessProperty));
                }
            }

            DefaultClaimSet customClaimSet = new DefaultClaimSet(claimList.ToArray());

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new CustomTokenAuthorizationPolicy(customClaimSet));

            return policies.AsReadOnly();
        }
    }
}
