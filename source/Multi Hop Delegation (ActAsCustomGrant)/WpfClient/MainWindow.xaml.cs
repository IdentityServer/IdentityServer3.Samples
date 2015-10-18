using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Windows;
using Thinktecture.Samples;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoginWebView _login;
        AuthorizeResponse _response;

        public MainWindow()
        {
            InitializeComponent();

            _login = new LoginWebView();
            _login.Done += _login_Done;

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _login.Owner = this;
        }

        void _login_Done(object sender, AuthorizeResponse e)
        {
            _response = e;
            Textbox1.Text = e.Raw;
        }

        private void LoginWithProfileButton_Click(object sender, RoutedEventArgs e)
        {
            RequestToken("openid profile", "code id_token");
        }

        private void LoginWithProfileAndAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            RequestToken("openid profile offline_access WebApi1", "code id_token token");
        }

        private void RequestToken(string scope, string responseType)
        {
            var request = new AuthorizeRequest(Constants.AuthorizeEndpoint);
            var startUrl = request.CreateAuthorizeUrl(
                clientId: "hybridclient",
                responseType: responseType,
                scope: scope,
                redirectUri: "oob://localhost/wpfclient",
                state: "random_state",
                nonce: "random_nonce");

            _login.Show();
            _login.Start(new Uri(startUrl), new Uri("oob://localhost/wpfclient"));
        }

        private async void UseCodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response != null && _response.Values.ContainsKey("code"))
            {
                var client = new TokenClient(
                    Constants.TokenEndpoint,
                    "hybridclient",
                    "secret");

                var response = await client.RequestAuthorizationCodeAsync(
                    _response.Code,
                    "oob://localhost/wpfclient");

                Textbox1.Text = response.Json.ToString();
            }
        }

        private async void CallUserInfo_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(Constants.UserInfoEndpoint)
            };

            if (_response != null && _response.Values.ContainsKey("access_token"))
            {  
                client.SetBearerToken(_response.AccessToken);
            }

            var response = await client.GetAsync("");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                Textbox1.Text = JObject.Parse(json).ToString();
            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString());
            }
        }

        private void ShowIdTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response.Values.ContainsKey("id_token"))
            {
                var viewer = new IdentityTokenViewer();
                viewer.IdToken = _response.Values["id_token"];
                viewer.Show();
            }
        }

        private void ShowAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response.Values.ContainsKey("access_token"))
            {
                var viewer = new IdentityTokenViewer();
                viewer.IdToken = _response.Values["access_token"];
                viewer.Show();
            }
        }

        private async void CallServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:44334/")
            };

            if (_response != null && _response.Values.ContainsKey("access_token"))
            {
                client.SetBearerToken(_response.AccessToken);
            }

            var response = await client.GetAsync("test");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                Textbox1.Text = json;
            }
            else
            {
                MessageBox.Show(response.StatusCode.ToString());
            }
        }

        private async void ValidateIdTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response != null && _response.Values.ContainsKey("id_token"))
            {
                var client = new HttpClient();

                var response = await client.GetAsync(Constants.IdentityTokenValidationEndpoint + "?token=" + _response.Values["id_token"] + "&client_id=hybridclient");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Textbox1.Text = JObject.Parse(json).ToString();
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }
            }
        }
    }
}