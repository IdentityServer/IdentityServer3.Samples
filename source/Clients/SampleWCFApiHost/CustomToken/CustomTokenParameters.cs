using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading.Tasks;

namespace SampleWCFApiHost.CustomToken
{
    /// <summary>
    /// CustomTokenParameters for use with the AccessToken
    /// </summary>
    public class CustomTokenParameters : SecurityTokenParameters
    {
        string issuer;

        public CustomTokenParameters() : this((string)null) { }

        public CustomTokenParameters(string issuer)
            : base()
        {
            this.issuer = issuer;
        }

        protected CustomTokenParameters(CustomTokenParameters other)
            : base(other)
        {
            this.issuer = other.issuer;
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new CustomTokenParameters(this);
        }

        public string Issuer
        {
            get { return this.issuer; }
        }

        protected override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = Constants.CustomTokenType;
            return;
        }

        // An AccessToken has no crypto, no windows identity and supports only client authentication
        protected override bool HasAsymmetricKey { get { return false; } }
        protected override bool SupportsClientAuthentication { get { return true; } }
        protected override bool SupportsClientWindowsIdentity { get { return false; } }
        protected override bool SupportsServerAuthentication { get { return false; } }

        protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (referenceStyle == SecurityTokenReferenceStyle.Internal)
            {
                return token.CreateKeyIdentifierClause<LocalIdKeyIdentifierClause>();
            }
            else
            {
                throw new NotSupportedException("External references are not supported for AccessTokens");
            }
        }
    }
}
