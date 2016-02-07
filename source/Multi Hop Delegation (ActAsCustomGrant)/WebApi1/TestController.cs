using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi1
{
    [Route("test")]
    public class TestController : ApiController
    {
        public string Get()
        {
            TokenClient tokenClient = new TokenClient(
                "http://localhost:44333/connect/token",
                "WebApi1",
                "4B79A70F-3919-435C-B46C-571068F7AF37"
            );

            var caller = User as ClaimsPrincipal;

            var customParams = new Dictionary<string, string>
            {
                { "token", caller.FindFirst("token").Value }
            };

            var tokenResponse = tokenClient.RequestCustomGrantAsync("act-as", "WebApi2", customParams).Result;

            HttpClient client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            return client.GetStringAsync("http://localhost:44335/test").Result;
        }
    }
}
