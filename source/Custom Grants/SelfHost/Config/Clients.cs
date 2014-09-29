using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Host.Config
{
    public class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Custom Grant Client",
                    Enabled = true,
                    ClientId = "client",
                    ClientSecret = "secret",
                    Flow = Flows.Custom,
                    
                    ScopeRestrictions = new List<string>
                    { 
                        "read",
                        "write",
                    },

                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 3600,
                }
            };
        }
    }
}