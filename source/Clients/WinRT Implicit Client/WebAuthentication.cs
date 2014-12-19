using System;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Windows.Security.Authentication.Web;

namespace Thinktecture.IdentityModel.WinRT
{
    public static class WebAuthentication
    {
        public async static Task<AuthorizeResponse> DoImplicitFlowAsync(Uri endpoint, string clientId, string responseType, string scope)
        {
            return await DoImplicitFlowAsync(
                endpoint, 
                clientId, 
                responseType,
                scope, 
                WebAuthenticationBroker.GetCurrentApplicationCallbackUri());
        }

        public async static Task<AuthorizeResponse> DoImplicitFlowAsync(
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
                var result = await WebAuthenticationBroker.AuthenticateAsync(
                        WebAuthenticationOptions.None,
                        new Uri(startUri));

                if (result.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    return new AuthorizeResponse(result.ResponseData);
                }
                else if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
                {
                    throw new Exception("User cancelled authentication");
                }
                else if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    throw new Exception("HTTP Error returned by AuthenticateAsync() : " + result.ResponseErrorDetail.ToString());
                }
                else
                {
                    throw new Exception("Error returned by AuthenticateAsync() : " + result.ResponseStatus.ToString());
                }
            }
            catch
            {
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                throw;
            }
        }
    }
}