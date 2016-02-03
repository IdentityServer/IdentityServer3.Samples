using IdentityModel.Client;
using System.Windows;

namespace WpfOidcClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoginWebView _login;
        const string AUTHORITY = "https://localhost:44333/core";

        public MainWindow()
        {
            InitializeComponent();

            _login = new LoginWebView();
            _login.Done += _login_Done;

            Loaded += MainWindow_Loaded;
            IdentityTextBox.Visibility = Visibility.Hidden;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _login.Owner = this;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        void _login_Done(object sender, AuthorizeResponse e)
        {
           
        }
    }
}