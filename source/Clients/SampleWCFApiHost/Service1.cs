using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.ServiceModel;

namespace SampleWCFApiHost
{
    public class Service1 : IService1
    {
        public string GetIdentityData()
        {
            var claims = ServiceSecurityContext.Current.AuthorizationContext.ClaimSets;

            string result = "";
            foreach (ClaimSet claimSet in claims)
            {                
                foreach(System.IdentityModel.Claims.Claim claim in claimSet)
                { 
                    result += "Claim Type: " + claim.ClaimType.ToString();
                    result += ", Claim Value: " + claim.Resource.ToString();
                    result += "\n\r";
                }
            }

            return string.Format("{0}", result);
            //return GetCallerScopes();
        }

        #region claim processing methods

        bool TryGetStringClaimValue(ClaimSet claimSet, string claimType, out string claimValue)
        {
            claimValue = null;
            
            IEnumerable<Claim> matchingClaims = claimSet.FindClaims(claimType, Rights.PossessProperty);
            if (matchingClaims == null)
                return false;

            foreach(var claim in matchingClaims)
            {
                claimValue += (claim.Resource == null) ? null : claim.Resource.ToString() + " ";
            }
            return true;
        }

        string GetCallerScopes()
        {
            foreach (ClaimSet claimSet in ServiceSecurityContext.Current.AuthorizationContext.ClaimSets)
            {
                string AccessToken = null;
                if (TryGetStringClaimValue(claimSet, "scope", out AccessToken))
                {
                    return String.Format("Scopes: '{0}'", AccessToken);
                }
            }
            return "Scope is not known";
        }
        #endregion
    }
}
