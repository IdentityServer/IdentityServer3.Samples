using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;

namespace SampleWCFApiHost.CustomToken
{
    public class CustomTokenClientCredentialsSecurityTokenManager : ClientCredentialsSecurityTokenManager
    {
        CustomTokenClientCredentials CustomClientCredentials;

        public CustomTokenClientCredentialsSecurityTokenManager(CustomTokenClientCredentials CustomClientCredentials)
            : base(CustomClientCredentials)
        {
            this.CustomClientCredentials = CustomClientCredentials;
        }

        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
        {
            // handle this token for Custom
            if (tokenRequirement.TokenType == Constants.CustomTokenType)
                return new CustomTokenProvider(this.CustomClientCredentials.AccessToken);
            // return server cert
            else if (tokenRequirement is InitiatorServiceModelSecurityTokenRequirement)
            {
                if (tokenRequirement.TokenType == SecurityTokenTypes.X509Certificate)
                {
                    return new X509SecurityTokenProvider(CustomClientCredentials.ServiceCertificate.DefaultCertificate);
                }
            }

            return base.CreateSecurityTokenProvider(tokenRequirement);
        }

        public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
        {
            return new CustomSecurityTokenSerializer(version);
        }
    }
}
