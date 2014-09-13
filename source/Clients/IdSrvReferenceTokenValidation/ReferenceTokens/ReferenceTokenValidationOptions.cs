using Microsoft.Owin.Security;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class ReferenceTokenValidationOptions : AuthenticationOptions
    {
        public ReferenceTokenValidationOptions() : base("IdSrvReferenceToken")
        { }
        
        public string Authority { get; set; }
    }
}