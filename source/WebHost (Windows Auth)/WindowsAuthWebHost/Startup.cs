using IdentityServer.WindowsAuthentication.Configuration;
using Microsoft.Owin;
using Owin;
using Configuration;

[assembly: OwinStartup(typeof(WindowsAuthWebHost.Startup))]

namespace WindowsAuthWebHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWindowsAuthenticationService(new WindowsAuthenticationOptions
            {
                IdpReplyUrl = "https://localhost:44333/was",
                SigningCertificate = Certificate.Load(),
                EnableOAuth2Endpoint = false
            });
        }
    }
}