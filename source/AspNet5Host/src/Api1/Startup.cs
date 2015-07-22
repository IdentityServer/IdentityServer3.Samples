using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using System.Collections.Generic;
using System.IdentityModel.Tokens;

namespace Api1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseOAuthBearerAuthentication(options =>
            {
                options.Authority = "https://localhost:44300";
                options.Audience = "https://localhost:44300/resources";
                options.AutomaticAuthentication = true;
            });

            app.UseMiddleware<RequiredScopesMiddleware>(new List<string> { "api1" });

            app.UseMvc();
        }
    }
}