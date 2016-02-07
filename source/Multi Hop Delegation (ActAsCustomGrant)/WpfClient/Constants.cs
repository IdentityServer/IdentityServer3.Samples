using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient
{
    public static class Constants
    {
        public const string BaseAddress = "http://localhost:44333";

        public const string AuthorizeEndpoint = BaseAddress + "/connect/authorize";
        public const string LogoutEndpoint = BaseAddress + "/connect/endsession";
        public const string TokenEndpoint = BaseAddress + "/connect/token";
        public const string UserInfoEndpoint = BaseAddress + "/connect/userinfo";
        public const string IdentityTokenValidationEndpoint = BaseAddress + "/connect/identitytokenvalidation";
        public const string TokenRevocationEndpoint = BaseAddress + "/connect/revocation";
    }
}
