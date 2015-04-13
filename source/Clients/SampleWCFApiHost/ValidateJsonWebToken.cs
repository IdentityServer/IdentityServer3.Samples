using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using SampleWCFApiHost;
using SampleWCFApiHost.Config;

namespace SampleWCFApiHost
{
    public class ValidateJsonWebToken : ServiceAuthorizationManager
    {
        public override bool CheckAccess(OperationContext operationContext)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;

            if (operationContext.RequestContext.RequestMessage.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers["Authorization"]))
                {
                    return false;
                }

                if (!httpRequestMessage.Headers["Authorization"].StartsWith("Bearer "))
                {
                    return false;
                }
                
                string jwt = httpRequestMessage.Headers["Authorization"].Split(' ')[1];
                if (string.IsNullOrEmpty(jwt))
                {
                    return false;
                }
                
                ClaimsPrincipal claimsPrincipal = ValidateToken(jwt);
                SetPrincipal(operationContext, claimsPrincipal);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Reads and validates a token encoded in JSON Compact serialized format.
        /// </summary>
        /// <param name="securityToken">A 'JSON Web Token' (JWT) that has been encoded as a JSON object. May be signed using 'JSON Web Signature' (JWS).</param>
        /// <returns>A <see cref="ReadOnlyCollection<ClaimsIdentity>"/> from the jwt.</returns>
        private ClaimsPrincipal ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            string BaseAddress = "https://idsrv3.com";

            var parameters = new TokenValidationParameters
            {
                ValidAudience = BaseAddress + "/resources",
                ValidIssuer = BaseAddress,
                IssuerSigningToken = new X509SecurityToken(Certificate.Get())
            };

            SecurityToken validatedToken;
            ClaimsPrincipal validated = handler.ValidateToken(token, parameters, out validatedToken);

            return validated;
        }


        private void SetPrincipal(OperationContext operationContext, ClaimsPrincipal principal)
        {
            var properties = operationContext.ServiceSecurityContext.AuthorizationContext.Properties;

            if (!properties.ContainsKey("Principal"))
            {
                properties.Add("Principal", principal);
            }
            else
            {
                properties["Principal"] = principal;
            }
        }
    }
}