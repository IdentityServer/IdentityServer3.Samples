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
using WpfOidcClientPop.OidcClient;

namespace WpfOidcClientPop
{
    public partial class LoginWebView : Window
    {
        private readonly string _doneUrl;

        public event EventHandler<AuthorizeResponse> Done;

        public LoginWebView(string doneUrl)
        {
            _doneUrl = doneUrl;

            InitializeComponent();

            webView.Navigating += webView_Navigating;
            Closing += LoginWebView_Closing;
        }

        public void Navigate(string url)
        {
            this.Visibility = Visibility.Visible;
            webView.Navigate(url);
        }

        void LoginWebView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void webView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            AuthorizeResponse response;

            if (e.Uri.ToString().StartsWith(_doneUrl))
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

                Done?.Invoke(this, response);
            }
        }
    }
}