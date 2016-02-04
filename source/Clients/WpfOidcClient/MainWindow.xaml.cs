using IdentityModel.Client;
using Sample;
using System.Text;
using System.Windows;
using WpfOidcClient.OidcClient;

namespace WpfOidcClient
{
    public partial class MainWindow : Window
    {
        LoginWebView _login;

        public MainWindow()
        {
            InitializeComponent();

            _login = new LoginWebView();
            _login.Done += _login_Done;

            Loaded += MainWindow_Loaded;
            //IdentityTextBox.Visibility = Visibility.Hidden;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _login.Owner = this;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new OidcSettings
            {
                Authority = "https://localhost:44333/core",
                ClientId = "wpf.hybrid",
                ClientSecret = "secret",
                RedirectUri = "http://localhost/wpf.hybrid",
                Scope = "openid profile write",
                LoadUserProfile = true
            };

            await _login.LoginAsync(settings);
        }

        void _login_Done(object sender, LoginResult e)
        {
            if (e.Success)
            {
                var sb = new StringBuilder(128);

                foreach (var claim in e.User.Claims)
                {
                    sb.AppendLine($"{claim.Type}: {claim.Value}");
                }

                sb.AppendLine();

                sb.AppendLine($"Identity token: {e.IdentityToken}");
                sb.AppendLine($"Access token: {e.AccessToken}");
                sb.AppendLine($"Access token expiration: {e.AccessTokenExpiration}");
                sb.AppendLine($"Refresh token: {e?.RefreshToken ?? "none" }");

                IdentityTextBox.Text = sb.ToString();
            }
            else
            {
                IdentityTextBox.Text = e.ErrorMessage;
            }


        }
    }
}