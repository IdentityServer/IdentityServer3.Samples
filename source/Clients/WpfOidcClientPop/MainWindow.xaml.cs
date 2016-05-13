using IdentityModel;
using IdentityModel.Client;
using IdentityModel.HttpSigning;
using IdentityModel.Jwt;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfOidcClientPop.OidcClient;

namespace WpfOidcClientPop
{
    public partial class MainWindow : Window
    {
        LoginWebView _login;
        OpenIdConnectConfiguration _config;
        OidcSettings _settings;

        string _nonce;
        string _verifier;

        RSACryptoServiceProvider _provider;
        LoginResult _result;

        public MainWindow()
        {
            InitializeComponent();

            _settings = new OidcSettings
            {
                Authority = "https://localhost:44333/core",
                ClientId = "wpf.hybrid.pop",
                ClientSecret = "secret",
                RedirectUri = "http://localhost/wpf.hybrid.pop",
                Scope = "openid profile write offline_access",
                LoadUserProfile = true
            };

            _login = new LoginWebView(_settings.RedirectUri);
            _login.Done += _login_Done;

            Loaded += MainWindow_Loaded;
            //IdentityTextBox.Visibility = Visibility.Hidden;
        }

        private async void _login_Done(object sender, AuthorizeResponse e)
        {
            _result = await ValidateResponseAsync(e);
            ShowTokenResult();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _login.Owner = this;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_config == null)
            {
                await LoadOpenIdConnectConfigurationAsync();
            }

            _login.Navigate(CreateUrl());
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

            _provider = JwkNetExtensions.CreateProvider();
            var jwk = _provider.ToJsonWebKey();

            // code eintauschen gegen tokens
            var tokenClient = new TokenClient(
                _config.TokenEndpoint,
                _settings.ClientId,
                _settings.ClientSecret);

            var tokenResponse = await tokenClient.RequestAuthorizationCodePopAsync(
                code: response.Code,
                redirectUri: _settings.RedirectUri,
                codeVerifier: _verifier,
                algorithm: jwk.Alg,
                key: jwk.ToJwkString());

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

        void ShowTokenResult()
        {
            if (_result.Success)
            {
                var sb = new StringBuilder(128);

                if (_result.User != null)
                {
                    foreach (var claim in _result.User.Claims)
                    {
                        sb.AppendLine($"{claim.Type}: {claim.Value}");
                    }

                    sb.AppendLine();
                }

                sb.AppendLine($"Identity token: {_result.IdentityToken}");
                sb.AppendLine($"Access token: {_result.AccessToken}");
                sb.AppendLine($"Access token expiration: {_result.AccessTokenExpiration}");
                sb.AppendLine($"Refresh token: {_result?.RefreshToken ?? "none" }");

                IdentityTextBox.Text = sb.ToString();
            }
            else
            {
                IdentityTextBox.Text = _result.ErrorMessage;
            }
        }

        private async void api_Click(object sender, RoutedEventArgs e)
        {
            var baseAddress = Sample.Constants.AspNetWebApiSampleApiUsingPoP;

            var signature = new RS256Signature(_provider);
            var signingHandler = new HttpSigningMessageHandler(signature);

            var client = new HttpClient(signingHandler)
            {
                BaseAddress = new Uri(baseAddress)
            };
            
            client.SetToken("PoP", _result?.AccessToken);

            var response = await client.GetAsync("identity");

            var sb = new StringBuilder(128);
            sb.AppendLine($"{(int)response.StatusCode}, {response.StatusCode}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var values = JArray.Parse(json);
                foreach (JObject item in values)
                {
                    sb.AppendLine($"{item["type"].ToString()}, {item["value"].ToString()}");
                }
            }
            else
            {
                sb.AppendLine(json);
            }

            IdentityTextBox.Text = sb.ToString();
        }

        private async void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (_config == null)
            {
                await LoadOpenIdConnectConfigurationAsync();
            }

            var tokenClient = new TokenClient(
                _config.TokenEndpoint,
                _settings.ClientId,
                _settings.ClientSecret);

            _provider = JwkNetExtensions.CreateProvider();

            var jwk = _provider.ToJsonWebKey();

            var tokenResponse = await tokenClient.RequestRefreshTokenPopAsync(
                refreshToken: _result?.RefreshToken,
                algorithm: jwk.Alg,
                key: jwk.ToJwkString());

            if (tokenResponse.IsError)
            {
                _result = new LoginResult { ErrorMessage = tokenResponse.Error };
            }
            else
            {
                _result = new LoginResult
                {
                    Success = true,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    IdentityToken = tokenResponse.IdentityToken,
                    AccessTokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
                };
            }

            ShowTokenResult();
        }
    }
}