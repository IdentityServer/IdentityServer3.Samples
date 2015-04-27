using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace SampleWCFApiHost.CustomToken
{
    /// <summary>
    /// CustomTokenProvider for use with the AccessToken
    /// </summary>
    class CustomTokenProvider : SecurityTokenProvider
    {
        string AccessToken;

        public CustomTokenProvider(string accessToken)
            : base()
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }
            this.AccessToken = accessToken;
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            SecurityToken result = new CustomToken(this.AccessToken);
            return result;
        }
    }
}
