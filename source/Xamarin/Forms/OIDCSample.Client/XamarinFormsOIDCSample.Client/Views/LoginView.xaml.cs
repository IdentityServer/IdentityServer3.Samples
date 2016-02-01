using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace XamarinFormsOIDCSample.Client.Views
{
    public partial class LoginView : ContentPage
    {
        private AuthorizeResponse _authResponse;
        private string _currentCSRFToken;

        public LoginView()
        {
            InitializeComponent();
            btnGetIdToken.Clicked += GetIdToken;
            btnGetAccessToken.Clicked += GetAccessToken;
            btnGetIdTokenAndAccessToken.Clicked += GetIdTokenAndAccessToken;
            wvLogin.Navigating += WvLogin_Navigating;
          }

        private void WvLogin_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.Contains("https://xamarin-oidc-sample/redirect"))
            {
                wvLogin.IsVisible = false;

                // parse response
                _authResponse = new AuthorizeResponse(e.Url);

                // CSRF check
                var state = _authResponse.Values["state"];
                if (state != _currentCSRFToken)
                {
                    txtResult.Text = "CSRF token doesn't match";
                    return;
                }

                string decodedTokens = "";
                // decode tokens
                if (!string.IsNullOrWhiteSpace(_authResponse.IdentityToken))
                {
                    decodedTokens += "Identity token \r\n";
                    decodedTokens += DecodeToken(_authResponse.IdentityToken) + "\r\n";
                }

                if (!string.IsNullOrWhiteSpace(_authResponse.AccessToken))
                {
                    decodedTokens += "Access token \r\n";
                    decodedTokens += DecodeToken(_authResponse.AccessToken);
                }

                txtResult.Text = decodedTokens;
            }
        }

        private void GetIdToken(object sender, EventArgs e)
        {
            // id_tokens don't contain resource scopes, only ask for
            // openid and profile
            StartFlow("id_token", "openid profile");
        }

        private void GetAccessToken(object sender, EventArgs e)
        {
            // access tokens are for resource authorization, only ask for
            // resource scopes
            StartFlow("token", "read write");
        }

        private void GetIdTokenAndAccessToken(object sender, EventArgs e)
        {
            // when asking both, we can ask for identity-related scopes
            // as well as resource scopes
            StartFlow("id_token token", "openid profile read write");
        }

        public void StartFlow(string responseType, string scope)
        {
            // create URI to authorize endpoint - use WebHost or SelfHost from the 
            // samples solution.
            var authorizeRequest =
                new AuthorizeRequest("https://localhost:44333/core/connect/authorize");

            // dictionary with values for the authorize request
            var dic = new Dictionary<string, string>();
            dic.Add("client_id", "implicitclient");
            dic.Add("response_type", responseType);
            dic.Add("scope", scope);
            dic.Add("redirect_uri", "https://xamarin-oidc-sample/redirect");
            dic.Add("nonce", Guid.NewGuid().ToString("N"));

            // add CSRF token to protect against cross-site request forgery attacks.
            _currentCSRFToken = Guid.NewGuid().ToString("N");
            dic.Add("state", _currentCSRFToken);

            var authorizeUri = authorizeRequest.Create(dic);

            // or use CreateAuthorizeUrl, passing in the values we defined in the dictionary. 
            // authorizeRequest.CreateAuthorizeUrl("implicitclient", ...);

            wvLogin.Source = authorizeUri;
            wvLogin.IsVisible = true;
        }

        public static string DecodeToken(string token)
        {
            var parts = token.Split('.');

            string partToConvert = parts[1];
            partToConvert = partToConvert.Replace('-', '+');
            partToConvert = partToConvert.Replace('_', '/');
            switch (partToConvert.Length % 4)
            {
                case 0:
                    break;
                case 2:
                    partToConvert += "==";
                    break;
                case 3:
                    partToConvert += "=";
                    break;
            }

            var partAsBytes = Convert.FromBase64String(partToConvert);
            var partAsUTF8String = Encoding.UTF8.GetString(partAsBytes, 0, partAsBytes.Count());

            return JObject.Parse(partAsUTF8String).ToString();
        }
    }
}
