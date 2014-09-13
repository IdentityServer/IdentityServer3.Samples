using Microsoft.Owin.Security;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class SelfContainedTokenValidationOptions : AuthenticationOptions
    {
        public SelfContainedTokenValidationOptions()
            : base("IdSrvSelfContainedToken")
        { }

        // either provide base url for discovery
        public string Authority { get; set; }

        // or set issuer and cert manually
        public string IssuerName { get; set; }
        public X509Certificate2 SigningCertificate { get; set; }
    }
}