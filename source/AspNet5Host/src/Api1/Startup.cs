using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

namespace Api1
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
            app.UseOAuthBearerAuthentication(options =>
            {
                options.Authority = "https://localhost:44300";
                options.Audience = "https://localhost:44300/resources";
                options.AutomaticAuthentication = true;
            });

            app.UseMvc();
        }
    }
}
