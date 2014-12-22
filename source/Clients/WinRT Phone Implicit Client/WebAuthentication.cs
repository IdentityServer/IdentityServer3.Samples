using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Windows.Security.Authentication.Web;

namespace WinRT_Phone_Implicit_Client
{
    public static class WebAuthentication
    {
        public static void DoImplicitFlowAsync(Uri endpoint, string clientId, string responseType, string scope)
        {
            DoImplicitFlowAsync(
                endpoint,
                clientId,
                responseType,
                scope,
                WebAuthenticationBroker.GetCurrentApplicationCallbackUri());
        }

        public static void DoImplicitFlowAsync(
            Uri endpoint,
            string clientId,
            string responseType,
            string scope,
            Uri redirectUri)
        {
            var client = new OAuth2Client(endpoint);
            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");

            var startUri = client.CreateAuthorizeUrl(
                clientId: clientId,
                responseType: responseType,
                scope: scope,
                redirectUri: redirectUri.AbsoluteUri,
                state: state,
                nonce: nonce);

            try
            {

                // On Windows Phone 8.1, the AuthenticateAsync method isn't implemented on 
                // the WebAuthenticationBroker.  Therefor, AuthenticateAndContinue is used.  
                // 
                // Callback = ContinueWebAuthentication in MainPage.xaml.cs

                WebAuthenticationBroker.AuthenticateAndContinue(
                    new Uri(startUri),
                   redirectUri,
                     null,
                     WebAuthenticationOptions.None
                    ); 
            }
            catch
            {
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                throw;
            }
        }
    }
}
