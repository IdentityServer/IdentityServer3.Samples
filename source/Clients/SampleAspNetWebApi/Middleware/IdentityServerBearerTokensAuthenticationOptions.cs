using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class IdentityServerBearerTokenAuthenticationOptions : AuthenticationOptions
    {
        public IdentityServerBearerTokenAuthenticationOptions() : base("Bearer")
        {
            ValidationMode = ValidationMode.ValidationEndpoint;
            RequiredScopes = Enumerable.Empty<string>();

            ValidationCacheDuration = TimeSpan.FromMinutes(5);
        }

        // common for local and validation endpoint
        public ValidationMode ValidationMode { get; set; }
        public string Authority { get; set; }
        public IEnumerable<string> RequiredScopes { get; set; }

        // validation endoint specific
        public bool CacheValidationResult { get; set; }
        public IValidationCache ValidationCache { get; set; }
        public TimeSpan ValidationCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the authentication provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IOAuthBearerAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the a certificate validator to use to validate the metadata endpoint.
        /// </summary>
        /// <value>
        /// The certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with the metadata endpoint.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TokenValidationParameters"/> used to determine if a token is valid.
        /// </summary>
        public TokenValidationParameters TokenValidationParameters { get; set; }

        /// <summary>
        /// A System.IdentityModel.Tokens.SecurityTokenHandler designed for creating and validating Json Web Tokens.
        /// </summary>
        public JwtSecurityTokenHandler TokenHandler { get; set; }
    }
}