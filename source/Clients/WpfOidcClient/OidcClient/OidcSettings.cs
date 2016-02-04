using System.Collections.Generic;

namespace WpfOidcClient.OidcClient
{
    public class OidcSettings
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; }

        public bool LoadUserProfile { get; set; } = false;
        public bool FilterClaims { get; set; } = true;

        public List<string> FilterClaimTypes { get; set; } = new List<string>
        {
            "iss",
            "exp",
            "nbf",
            "aud",
            "nonce",
            "c_hash",
            "iat",
            "auth_time"
        };
    }
}