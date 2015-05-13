using System.IdentityModel.Selectors;
using System.ServiceModel.Security;

namespace SampleWCFApiHost.CustomToken
{
    public class CustomServiceCredentialsSecurityTokenManager : ServiceCredentialsSecurityTokenManager
    {
        CustomTokenServiceCredentials CustomServiceCredentials;

        public CustomServiceCredentialsSecurityTokenManager(CustomTokenServiceCredentials CustomServiceCredentials)
            : base(CustomServiceCredentials)
        {
            this.CustomServiceCredentials = CustomServiceCredentials;
        }

        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (tokenRequirement.TokenType == Constants.CustomTokenType)
            {
                outOfBandTokenResolver = null;
                return new CustomTokenAuthenticator();
            }

            return base.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
        }

        public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
        {
            return new CustomSecurityTokenSerializer(version);
        }
    }
}
