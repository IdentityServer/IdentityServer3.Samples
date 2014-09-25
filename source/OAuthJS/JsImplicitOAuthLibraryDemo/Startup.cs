using Owin;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web.Http;
using Thinktecture.IdentityModel.Tokens;

namespace JsImplicitOAuthLibraryDemo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
            
            var cert = Thinktecture.IdentityModel.X509.LocalMachine.TrustedPeople.SubjectDistinguishedName.Find("CN=idsrv3test", false).First();
            app.UseJsonWebToken(
                "https://idsrv3.com",
                "https://idsrv3.com/resources",
                cert);

            var config = new HttpConfiguration();
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter("Bearer"));
            config.Filters.Add(new AuthorizeAttribute());

            config.MapHttpAttributeRoutes();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.Remove(config.Formatters.FormUrlEncodedFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            app.UseWebApi(config);
        }
    }
}