using SampleWCFApiHost.Config;
using SampleWCFApiHost.CustomToken;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace CustomJwtSecurityTokenHandler
{
    public class CustomJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        /// <summary>
        /// Reads and validates a token encoded in JSON Compact serialized format.
        /// </summary>
        /// <param name="securityToken">A 'JSON Web Token' (JWT) that has been encoded as a JSON object. May be signed using 'JSON Web Signature' (JWS).</param>
        /// <returns>A <see cref="ReadOnlyCollection<ClaimsIdentity>"/> from the jwt.</returns>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            CustomToken customToken = (CustomToken)token;

            // Set the validation parameters from the configuration.
            var validationParameters = new TokenValidationParameters
            {
                // Get the audiences that are expected.
                ValidAudiences = new string[] {"https://idsrv3.com/resources"},

                // Get the issuer that are expected.
                //ValidIssuers = issuingAuthority.Issuers,
                ValidIssuers = new List<string> { "https://idsrv3.com" },

                // Get the certificate to validate signing from the certificate store (if configured).
                IssuerSigningKey = new X509SecurityKey(Certificate.Get()),

                // Get the symmetric key token that is used to sign (if configured).
                // Did not get this one working though.
                IssuerSigningToken = new X509SecurityToken(Certificate.Get()),

                // Get how to validate the certificate.
                //CertificateValidator = Configuration.CertificateValidator,

                // Get if the token should be preserved.
                //SaveSigninToken = Configuration.SaveBootstrapContext
                SaveSigninToken = true                
            };

            // Call the correct validation method.
            SecurityToken validatedToken;
            ClaimsPrincipal validated = ValidateToken(customToken.AccessToken, validationParameters, out validatedToken);

            // Return the claim identities.
            ReadOnlyCollection<ClaimsIdentity> collection = new ReadOnlyCollection<ClaimsIdentity>(validated.Identities.ToList());

            return collection;
        }
    }
}