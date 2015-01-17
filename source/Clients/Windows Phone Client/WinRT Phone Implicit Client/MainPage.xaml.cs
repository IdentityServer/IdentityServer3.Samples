using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WinRT_Phone_Implicit_Client
{
    /// <summary>
    /// Main entry point for Windows Phone sample.  Note that if you're hosting idSrv on IIS Express 
    /// (using the developer certificate), you will need to ensure the emulator trusts that certificate.  
    ///
    /// There's two steps you need to complete for this: 
    ///    - The first is exporting the certificate from the machine you're currently working on.  
    ///    You only need to do this once.  
    ///    - The second is ensuring the phone emulator trusts this certificate. 
    ///    This must be done every time you restart the emulator.
    /// 
    /// To export the certificate:
    ///    - Open mmc and select the certificates plugin. (computer account)
    ///    - Right-click on the "localhost" certificate, choose "All Tasks->Export...".
    ///    - Click "next" without changing the defaults until you reach the "export file format" screen. Export as .P7B.
    ///    - Save the certificate as "localhost.p7b" in any location you can surf to from the Windows Phone app (for example, your
    ///    local IIS)
    /// 
    /// To trust the certificate:
    ///    - Start the emulator, open IE on the emulator & surf to the location where you saved the certficate.  You'll
    ///    be asked to install this certificate.  Do this, and you're done.
    /// </summary>
    public sealed partial class MainPage : Page, IWebAuthenticationContinuable
    {

        AuthorizeResponse _response;


        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void LoginOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            await StartFlowAsync("id_token", "openid");
        }

        private async Task StartFlowAsync(string responseType, string scope)
        {
            // clear textboxes
            Output.Text = "";
            IdToken.Text = "";
            AccessToken.Text = "";

            Exception exception = null;

            try
            {
                // response can't be awaited as the WebAuthBroker on Windows Phone
                // doesn't implement AuthenticateAsync - results are returned through
                // ContinueWebAuthentication 

                   WebAuthentication.DoImplicitFlowAsync(
                    new Uri(Constants.AuthorizeEndpoint),
                    "implicitclient",
                    responseType,
                    scope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                var md = new MessageDialog(exception.ToString());
                await md.ShowAsync();
            }
        }

        private string ParseToken(string token)
        {
            var parts = token.Split('.');
            var partAsBytes = Base64Url.Decode(parts[1]);
            var part = Encoding.UTF8.GetString(partAsBytes, 0, partAsBytes.Count());

            var jwt = JObject.Parse(part);
            return jwt.ToString();
        }

        private async void AccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            await StartFlowAsync("token", "read write");
        }

        private async void LoginAndAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            await StartFlowAsync("id_token token", "openid read write");
        }

        private async void LoginAndAccessTokenWithIdentityClaims_Click(object sender, RoutedEventArgs e)
        {
            await StartFlowAsync("id_token token", "openid read write idmgr");
        }


        public void ContinueWebAuthentication(Windows.ApplicationModel.Activation.WebAuthenticationBrokerContinuationEventArgs args)
        {
            Exception exception = null;

            if (args.WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                _response = new AuthorizeResponse(args.WebAuthenticationResult.ResponseData);
                
                Output.Text = _response.Raw;
                AccessToken.Text = _response.AccessToken == null ? "" : ParseToken(_response.AccessToken);
                IdToken.Text = _response.IdentityToken == null ? "" : ParseToken(_response.IdentityToken);
                
            }
            else if (args.WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.UserCancel)
            {
               exception = new Exception("User cancelled authentication");
            }
            else if (args.WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                exception = new Exception("HTTP Error on AuthenticateAndContinue() : " + args.WebAuthenticationResult.ResponseErrorDetail.ToString());
            }
            else
            {
                exception = new Exception("Error on AuthenticateAndContinue() : " + args.WebAuthenticationResult.ResponseStatus.ToString());
            }


            if (exception != null)
            {
                var md = new MessageDialog(exception.ToString());
                md.ShowAsync();
            }
        }



        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

     
    }
}
