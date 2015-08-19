using IdentityModel.Client;
using mshtml;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Thinktecture.Samples
{
    public partial class LoginWebView : Window
    {
        public AuthorizeResponse AuthorizeResponse { get; set; }
        public event EventHandler<AuthorizeResponse> Done;

        Uri _callbackUri;

        public LoginWebView()
        {
            InitializeComponent();
            webView.Navigating += webView_Navigating;

            Closing += LoginWebView_Closing;
        }

        void LoginWebView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        public void Start(Uri startUri, Uri callbackUri)
        {
            _callbackUri = callbackUri;
            webView.Navigate(startUri);
        }

        private void webView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(_callbackUri.AbsoluteUri))
            {
                if (e.Uri.AbsoluteUri.Contains("#"))
                {
                    AuthorizeResponse = new AuthorizeResponse(e.Uri.AbsoluteUri);
                }
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
                    AuthorizeResponse = new AuthorizeResponse(resultUrl);
                }

                e.Cancel = true;
                this.Visibility = Visibility.Hidden;

                if (Done != null)
                {
                    Done.Invoke(this, AuthorizeResponse);
                }
            }
        }
    }
}