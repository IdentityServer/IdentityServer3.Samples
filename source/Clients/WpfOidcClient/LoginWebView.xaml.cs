using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using mshtml;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfOidcClient.OidcClient;

namespace WpfOidcClient
{
    public partial class LoginWebView : Window
    {
        public event EventHandler<LoginResult> Done;

        OpenIdConnectConfiguration _config;
        OidcSettings _settings;

        string _nonce;
        string _verifier;

        public LoginWebView()
        {
            InitializeComponent();

            webView.Navigating += webView_Navigating;
            Closing += LoginWebView_Closing;
        }

        public async Task LoginAsync(OidcSettings settings)
        {
            _settings = settings;

            if (_config == null)
            {
                await LoadOpenIdConnectConfigurationAsync();
            }

            this.Visibility = Visibility.Visible;
            webView.Navigate(CreateUrl());
        }

        private string CreateUrl()
        {
            _nonce = CryptoRandom.CreateUniqueId(32);
            _verifier = CryptoRandom.CreateUniqueId(32);
            var challenge = _verifier.ToCodeChallenge();

            var request = new AuthorizeRequest(_config.AuthorizationEndpoint);

            return request.CreateAuthorizeUrl(
                clientId: _settings.ClientId,
                responseType: "code id_token",
                scope: _settings.Scope,
                redirectUri: _settings.RedirectUri,
                nonce: _nonce,
                responseMode: OidcConstants.ResponseModes.FormPost,
                codeChallenge: challenge,
                codeChallengeMethod: OidcConstants.CodeChallengeMethods.Sha256);
        }

        private async Task LoadOpenIdConnectConfigurationAsync()
        {
            var discoAddress = _settings.Authority + "/.well-known/openid-configuration";

            var manager = new ConfigurationManager<OpenIdConnectConfiguration>(discoAddress);
            _config = await manager.GetConfigurationAsync();
        }

        void LoginWebView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private async void webView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            AuthorizeResponse response;

            if (e.Uri.ToString().StartsWith(_settings.RedirectUri))
            {
                if (e.Uri.AbsoluteUri.Contains("#"))
                {
                    response = new AuthorizeResponse(e.Uri.AbsoluteUri);
                }
                // form_post support
                else
                {
                    var document = (IHTMLDocument3)((WebBrowser)sender).Document;
                    var inputElements = document.getElementsByTagName("INPUT").OfType<IHTMLElement>();
                    var resultUrl = "?";

                    foreach (var input in inputElements)
                    {
                        resultUrl += input.getAttribute("name") + "=";
                        resultUrl += input.getAttribute("value") + "&";
                    }

                    resultUrl = resultUrl.TrimEnd('&');
                    response = new AuthorizeResponse(resultUrl);
                }

                e.Cancel = true;
                this.Visibility = Visibility.Hidden;

                var loginResult = await ValidateResponseAsync(response);
                Done?.Invoke(this, loginResult);
            }
        }

        private async Task<LoginResult> ValidateResponseAsync(AuthorizeResponse response)
        {
            // id_token validieren
            var tokenClaims = ValidateIdentityToken(response.IdentityToken);

            if (tokenClaims == null)
            {
                return new LoginResult { ErrorMessage = "Invalid identity token." };
            }

            // nonce validieren
            var nonce = tokenClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.Nonce);

            if (nonce == null || !string.Equals(nonce.Value, _nonce, StringComparison.Ordinal))
            {
                return new LoginResult { ErrorMessage = "Inalid nonce." };
            }

            // c_hash validieren
            var c_hash = tokenClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.AuthorizationCodeHash);

            if (c_hash == null || ValidateCodeHash(c_hash.Value, response.Code) == false)
            {
                return new LoginResult { ErrorMessage = "Invalid code." };
            }

            // code eintauschen gegen tokens
            var tokenClient = new TokenClient(
                _config.TokenEndpoint,
                _settings.ClientId,
                _settings.ClientSecret);

            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(
                code: response.Code,
                redirectUri: _settings.RedirectUri,
                codeVerifier: _verifier);

            if (tokenResponse.IsError)
            {
                return new LoginResult { ErrorMessage = tokenResponse.Error };
            }

            // optional userinfo aufrufen
            var profileClaims = new List<Claim>();
            if (_settings.LoadUserProfile)
            {
                var userInfoClient = new UserInfoClient(
                    new Uri(_config.UserInfoEndpoint),
                    tokenResponse.AccessToken);

                var userInfoResponse = await userInfoClient.GetAsync();
                profileClaims = userInfoResponse.GetClaimsIdentity().Claims.ToList();
            }

            var principal = CreatePrincipal(tokenClaims, profileClaims);

            return new LoginResult
            {
                Success = true,
                User = principal,
                IdentityToken = response.IdentityToken,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                AccessTokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
            };
        }

        private ClaimsPrincipal CreatePrincipal(List<Claim> tokenClaims, List<Claim> profileClaims)
        {
            List<Claim> filteredClaims = new List<Claim>(tokenClaims);

            if (_settings.FilterClaims)
            {
                filteredClaims = tokenClaims.Where(c => !_settings.FilterClaimTypes.Contains(c.Type)).ToList();
            }

            var allClaims = new List<Claim>();
            allClaims.AddRange(filteredClaims);
            allClaims.AddRange(profileClaims);

            var id = new ClaimsIdentity(allClaims.Distinct(new ClaimComparer()), "OIDC");
            return new ClaimsPrincipal(id);
        }

        private bool ValidateCodeHash(string c_hash, string code)
        {
            using (var sha = SHA256.Create())
            {
                var codeHash = sha.ComputeHash(Encoding.ASCII.GetBytes(code));
                byte[] leftBytes = new byte[16];
                Array.Copy(codeHash, leftBytes, 16);

                var codeHashB64 = Base64Url.Encode(leftBytes);

                return string.Equals(c_hash, codeHashB64, StringComparison.Ordinal);
            }
        }

        private List<Claim> ValidateIdentityToken(string identityToken)
        {
            var tokens = new List<X509SecurityToken>(
                from key in _config.JsonWebKeySet.Keys
                select new X509SecurityToken(new X509Certificate2(Convert.FromBase64String(key.X5c.First()))));

            var parameter = new TokenValidationParameters
            {
                ValidIssuer = _config.Issuer,
                ValidAudience = _settings.ClientId,
                IssuerSigningTokens = tokens
            };

            JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
            var handler = new JwtSecurityTokenHandler();

            SecurityToken token;
            try
            {
                return handler.ValidateToken(identityToken, parameter, out token).Claims.ToList();
            }
            catch
            {
                return null;
            }
        }
    }
}