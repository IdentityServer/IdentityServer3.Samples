using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class IdentityServerBearerTokenAuthenticationOptions : AuthenticationOptions
    {
        public IdentityServerBearerTokenAuthenticationOptions() : base("Bearer")
        {
            TokenType = IdentityServerTokenType.Reference;
            RequiredScopes = Enumerable.Empty<string>();
        }

        // common for JWT and reference token
        public IdentityServerTokenType TokenType { get; set; }
        public string Authority { get; set; }
        public IEnumerable<string> RequiredScopes { get; set; }

        // reference token specific
        public TimeSpan ReferenceTokenCacheDuration { get; set; }

        // JWT specific
        public string IdentityServerName { get; set; }
        public IEnumerable<X509Certificate2> SigningCertificates { get; set; }

        //public TimeSpan MetadataRefreshInterval { get; set; }
    }
}