using System;
using System.Security.Claims;

namespace WpfOidcClientPop.OidcClient
{
    public class LoginResult
    {
        public bool Success { get; set; } = false;
        public string ErrorMessage { get; set; }

        public ClaimsPrincipal User { get; set; }

        public string AccessToken { get; set; }
        public string IdentityToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpiration { get; set; }
    }
}