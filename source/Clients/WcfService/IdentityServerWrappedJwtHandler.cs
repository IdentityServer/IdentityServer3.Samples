using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace WcfService
{
    class IdentityServerWrappedJwtHandler : Saml2SecurityTokenHandler
    {
        X509Certificate2 _signingCert;
        string _issuerName;
        private readonly string[] _requiredScopes;

        public IdentityServerWrappedJwtHandler(string authority, params string[] requiredScopes)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            var discoveryEndpoint = authority.EnsureTrailingSlash() + ".well-known/openid-configuration";
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(discoveryEndpoint);

            var config = configurationManager.GetConfigurationAsync().Result;
            _signingCert = new X509Certificate2(Convert.FromBase64String(config.JsonWebKeySet.Keys.First().X5c.First()));
            _issuerName = config.Issuer;
            _requiredScopes = requiredScopes;
        }

        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            var saml = token as Saml2SecurityToken;
            var samlAttributeStatement = saml.Assertion.Statements.OfType<Saml2AttributeStatement>().FirstOrDefault();
            var jwt = samlAttributeStatement.Attributes.Where(sa => sa.Name.Equals("jwt", StringComparison.OrdinalIgnoreCase)).SingleOrDefault().Values.Single();
            
            var parameters = new TokenValidationParameters
            {
                ValidAudience = _issuerName.EnsureTrailingSlash() + "resources",
                ValidIssuer = _issuerName,
                IssuerSigningToken = new X509SecurityToken(_signingCert)
            };

            SecurityToken validatedToken;
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(jwt, parameters, out validatedToken);
            
            var ci = new ReadOnlyCollection<ClaimsIdentity>(new List<ClaimsIdentity> { principal.Identities.First() });

            if (_requiredScopes.Any())
            {
                bool found = false;

                foreach (var scope in _requiredScopes)
                {
                    if (principal.HasClaim("scope", scope))
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    throw new SecurityTokenValidationException("Insufficient Scope");
                }
            }

            return ci;
        }
    }

    internal static class StringExtensions
    {
        public static string EnsureTrailingSlash(this string input)
        {
            if (!input.EndsWith("/"))
            {
                return input + "/";
            }

            return input;
        }
    }
}
