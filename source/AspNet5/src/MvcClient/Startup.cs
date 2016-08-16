using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace MvcClient
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseIISPlatformHandler();

            app.UseCookieAuthentication(options =>
            {
                options.AuthenticationScheme = "Cookies";
                options.AutomaticAuthenticate = true;
            });
            
            var oidcOptions = new OpenIdConnectOptions
            {
                AutomaticChallenge = true;
                AuthenticationScheme = "Oidc";
                SignInScheme = "Cookies";

                Authority = "http://localhost:18942/";
                RequireHttpsMetadata = false;

                ClientId = "mvc6";
                ResponseType = "id_token token";
                PostLogoutRedirectUri = "http://localhost:19276/"; // Has to be in PostLogoutRedirectUris in client settings on identity server
            };

            oidcOptions.Scope.Clear();
            oidcOptions.Scope.Add("openid");
            oidcOptions.Scope.Add("profile");
            oidcOptions.Scope.Add("email");
            oidcOptions.Scope.Add("api1");


            oidcOptions.Events = new OpenIdConnectEvents()
            {                
                OnRedirectToIdentityProvider = RedirectToIdentityProvider,
                OnAuthorizationCodeReceived = AuthorizationCodeReceived,
                OnTokenValidated = TokenValidated,
                OnMessageReceived = MessageReceived,
                OnAuthenticationFailed = AuthenticationFailed,
                OnRedirectToIdentityProviderForSignOut = RedirectToIdentityProviderForSignOut,
                OnRemoteSignOut = RemoteSignOut,
                OnTicketReceived = TicketReceived,
                OnTokenResponseReceived = TokenResponseReceived,
                OnUserInformationReceived = UserInformationReceived
                
            };
            
            app.UseOpenIdConnectAuthentication(oidcOptions);

            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();
        }

        #region Events
        public async Task RedirectToIdentityProvider(RedirectContext rc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("RedirectToIdentityProvider");
            });
        }

        public async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext acrc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("AuthorizationCodeReceived");
            });
        }

        public async Task TokenValidated(TokenValidatedContext tvc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("TokenValidated");
            });
        }

        public async Task MessageReceived(MessageReceivedContext mrc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("MessageReceived");
            });
        }

        public async Task AuthenticationFailed(AuthenticationFailedContext afc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("AuthenticationFailed");
            });
        }

        public async Task RemoteSignOut(RemoteSignOutContext rsoc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("RemoteSignOut");
            });
        }

        public async Task RedirectToIdentityProviderForSignOut(RedirectContext rc)
        {
            await Task.Run(() =>
            {
                rc.ProtocolMessage.IdTokenHint = rc.HttpContext.User.FindFirst("id_token").Value;
                Debug.WriteLine("RedirectToIdentityProviderForSignOut");
            });
        }

        public async Task TicketReceived(TicketReceivedContext trc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("TicketReceived");
            });
        }

        public async Task TokenResponseReceived(TokenResponseReceivedContext trrc)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("TokenResponseReceived");
            });
        }

        public async Task UserInformationReceived(UserInformationReceivedContext uirc)
        {
            await Task.Run(() =>
            {
                var identity = uirc.Ticket.Principal.Identity as ClaimsIdentity;
                identity.AddClaim(new Claim("access_token", uirc.ProtocolMessage.AccessToken));
                identity.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(Convert.ToDouble(uirc.ProtocolMessage.ExpiresIn)).ToLocalTime().ToString()));
                //identity.Claims.Append(new Claim("refresh_token", uirc.ProtocolMessage.RefreshToken));
                identity.AddClaim(new Claim("id_token", uirc.ProtocolMessage.IdToken));
                identity.Claims.Append(new Claim("sid", uirc.Ticket.Principal.FindFirst("sid").Value));


                Debug.WriteLine("UserInformationReceived");
            });
        }
        #endregion

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
