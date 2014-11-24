/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class IdentityServerBearerTokenAuthenticationOptions : AuthenticationOptions
    {
        public IdentityServerBearerTokenAuthenticationOptions() : base("Bearer")
        {
            ValidationMode = ValidationMode.ValidationEndpoint;
            RequiredScopes = Enumerable.Empty<string>();

            ClaimsCacheDuration = TimeSpan.FromMinutes(5);

            NameClaimType = "name";
            RoleClaimType = "role";
        }

        // common for local and validation endpoint
        public ValidationMode ValidationMode { get; set; }
        public string Authority { get; set; }
        public IEnumerable<string> RequiredScopes { get; set; }
        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }

        // validation endoint specific
        public bool CacheClaims { get; set; }
        public IClaimsCache ClaimsCache { get; set; }
        public TimeSpan ClaimsCacheDuration { get; set; }

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