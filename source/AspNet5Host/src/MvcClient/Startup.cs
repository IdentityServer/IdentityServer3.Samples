using System.Collections.Generic;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using System.IdentityModel.Tokens;

namespace MvcClient
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            // really? still?
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.Authority = "https://localhost:44300";
                options.ClientId = "mvc6";
                options.ResponseType = "id_token";
                options.Scope = "openid email profile";
                options.RedirectUri = "http://localhost:2221/";

                options.AutomaticAuthentication = true;
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}