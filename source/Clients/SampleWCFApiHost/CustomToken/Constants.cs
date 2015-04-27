using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWCFApiHost.CustomToken
{
    public class Constants
    {
        public const string CustomTokenType = "http://samples.microsoft.com/wcf/security/Extensibility/Tokens/CustomToken";

        internal const string CustomTokenPrefix = "ctp";
        internal const string CustomTokenNamespace = "http://samples.thinktecture.com/wcf/security/Extensibility/";
        internal const string CustomTokenName = "CustomToken";
        internal const string Id = "Id";
        internal const string WsUtilityPrefix = "wsu";
        internal const string WsUtilityNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

        internal const string CustomTokenElementName = "AccessToken";
    }
}
