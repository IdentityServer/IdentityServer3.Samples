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
using Thinktecture.IdentityModel.WinRT;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WinRT_Implicit_Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AuthorizeResponse _response;

        public MainPage()
        {
            this.InitializeComponent();
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
                _response = await WebAuthentication.DoImplicitFlowAsync(
                    new Uri(Constants.AuthorizeEndpoint),
                    "implicitclient",
                    responseType,
                    scope);
               
                Output.Text = _response.Raw;

                AccessToken.Text = _response.AccessToken == null ? "" : ParseToken(_response.AccessToken);
                IdToken.Text = _response.IdentityToken == null ? "" : ParseToken(_response.IdentityToken);
            
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
    }
}