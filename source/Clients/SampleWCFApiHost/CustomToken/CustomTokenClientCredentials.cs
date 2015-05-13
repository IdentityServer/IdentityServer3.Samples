using System;
using System.IdentityModel.Selectors;
using System.ServiceModel.Description;

namespace SampleWCFApiHost.CustomToken
{
    /// <summary>
    /// CustomClientCredentials for use with AccessToken
    /// </summary>
    public class CustomTokenClientCredentials : ClientCredentials
    {
        string accessToken;

        public CustomTokenClientCredentials(string accessToken)
            : base()
        {
            if (accessToken == null)
                throw new ArgumentNullException("accessToken");

            this.accessToken = accessToken;
        }

        public string AccessToken
        {
            get { return this.accessToken; }
        }

        protected override ClientCredentials CloneCore()
        {
            return new CustomTokenClientCredentials(this.accessToken);
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new CustomTokenClientCredentialsSecurityTokenManager(this);
        }
    }
}
