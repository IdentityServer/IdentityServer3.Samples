using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class ReferenceTokenProvider : AuthenticationTokenProvider
    {
        private HttpClient _client;
        private string _tokenValidationEndpoint;
        private string _authenticationType;

        public ReferenceTokenProvider(string authority, string authenticationType)
        {
            if (!authority.EndsWith("/"))
            {
                authority += "/";
            }

            authority += "connect/accesstokenvalidation";

            _tokenValidationEndpoint = authority + "?token={0}";
            _client = new HttpClient();
            _authenticationType = authenticationType;
        }

        public override async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var url = string.Format(_tokenValidationEndpoint, context.Token);
            
            var response = await _client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            var claims = new List<Claim>();

            foreach (var item in dictionary)
            {
                var values = item.Value as IEnumerable<object>;

                if (values == null)
                {
                    claims.Add(new Claim(item.Key, item.Value.ToString()));
                }
                else
                {
                    foreach (var value in values)
                    {
                        claims.Add(new Claim(item.Key, value.ToString()));
                    }
                }
            }

            context.SetTicket(new AuthenticationTicket(new ClaimsIdentity(claims, _authenticationType), new AuthenticationProperties()));
        }
    }
}