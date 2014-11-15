/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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
        private readonly HttpClient _client;
        private readonly string _tokenValidationEndpoint;
        private readonly IdentityServerBearerTokenAuthenticationOptions _options;

        public ReferenceTokenProvider(IdentityServerBearerTokenAuthenticationOptions options)
        {
            var authority = options.Authority;

            if (!authority.EndsWith("/"))
            {
                authority += "/";
            }

            authority += "connect/accesstokenvalidation";

            _tokenValidationEndpoint = authority + "?token={0}";
            _client = new HttpClient();
            _options = options;
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

            context.SetTicket(new AuthenticationTicket(new ClaimsIdentity(claims, _options.AuthenticationType), new AuthenticationProperties()));
        }
    }
}