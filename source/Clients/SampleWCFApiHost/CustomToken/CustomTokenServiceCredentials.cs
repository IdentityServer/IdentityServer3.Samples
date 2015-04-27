using System;
using System.IdentityModel.Selectors;
using System.ServiceModel.Description;

namespace SampleWCFApiHost.CustomToken
{
    /// <summary>
    /// CustomServiceCredentials for use with the JWT Token. It serves up a Custom SecurityTokenManager
    /// CustomServiceCredentialsSecurityTokenManager - that knows about our custom token.
    /// </summary>
    /// 
    public class CustomTokenServiceCredentials : ServiceCredentials
    {
        protected override ServiceCredentials CloneCore()
        {
            return new CustomTokenServiceCredentials();
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new CustomServiceCredentialsSecurityTokenManager(this);            
        }
    }
}
