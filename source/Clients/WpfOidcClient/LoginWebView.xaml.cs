using IdentityModel;
using IdentityModel.Client;
using mshtml;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfOidcClient
{
    public partial class LoginWebView : Window
    {
        public event EventHandler<AuthorizeResponse> Done;
        private string _endUrl;

        public LoginWebView()
        {
            InitializeComponent();
            webView.Navigating += webView_Navigating;

            Closing += LoginWebView_Closing;
        }

        public void Start(string startUrl, string endUrl)
        {
            _endUrl = endUrl;

            this.Visibility = System.Windows.Visibility.Visible;
            webView.Navigate(startUrl);
        }

        void LoginWebView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void webView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            AuthorizeResponse response;

            if (e.Uri.ToString().StartsWith(_endUrl))
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

                if (Done != null)
                {
                    Done.Invoke(this, response);
                }
            }
        }
    }
}