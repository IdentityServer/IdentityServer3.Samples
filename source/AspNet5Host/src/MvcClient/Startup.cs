using System.Collections.Generic;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Authentication;
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
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(options =>
            {
                options.AuthenticationScheme = "Cookies";
                options.AutomaticAuthentication = true;
            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.Authority = "https://localhost:44300";
                options.ClientId = "mvc6";
                options.ResponseType = "id_token token";
                options.ResponseMode = "form_post";
                options.Scope = "openid email profile api1";
                options.RedirectUri = "http://localhost:2221/";

                options.SignInScheme = "Cookies";
                options.AutomaticAuthentication = true;

                options.Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n =>
                        {
                            var incoming = n.AuthenticationTicket.Principal;

                            // create application identity
                            var id = new ClaimsIdentity("application", "given_name", "role");
                            id.AddClaim(incoming.FindFirst("sub"));
                            id.AddClaim(incoming.FindFirst("email"));
                            id.AddClaim(incoming.FindFirst("email_verified"));
                            id.AddClaim(incoming.FindFirst("given_name"));
                            id.AddClaim(incoming.FindFirst("family_name"));
                            id.AddClaim(new Claim("token", n.ProtocolMessage.AccessToken));

                            n.AuthenticationTicket = new AuthenticationTicket(
                                new ClaimsPrincipal(id),
                                n.AuthenticationTicket.Properties,
                                n.AuthenticationTicket.AuthenticationScheme);

                            // this skips nonce checking & cleanup 
                            // see https://github.com/aspnet/Security/issues/372
                            n.HandleResponse();
                            return Task.FromResult(0);
                        }
                };
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}