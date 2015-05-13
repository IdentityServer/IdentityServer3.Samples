using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace SampleWCFApiHost.CustomToken
{
    class CustomToken : SecurityToken
    {
        string accessToken;
        DateTime effectiveTime = DateTime.UtcNow;
        string id;
        ReadOnlyCollection<SecurityKey> securityKeys;

        public CustomToken(string accessToken) : this(accessToken, Guid.NewGuid().ToString()) { }

        public CustomToken(string accessToken, string id)
        {
            if (accessToken == null)
                throw new ArgumentNullException("accessToken");

            if (id == null)
                throw new ArgumentNullException("id");

            this.accessToken = accessToken;
            this.id = id;

            // the token is not capable of any crypto
            this.securityKeys = new ReadOnlyCollection<SecurityKey>(new List<SecurityKey>());
        }

        public string AccessToken { get { return this.accessToken; } }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys { get { return this.securityKeys; } }

        public override DateTime ValidFrom { get { return this.effectiveTime; } }
        public override DateTime ValidTo { get { return this.effectiveTime.AddMinutes(5); } }
        public override string Id { get { return this.id; } }
    }
}
