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
                    ClientSecrets = new List<ClientSecret>
                    {
                        new ClientSecret("secret".Sha256()),
                    },
                    
                    Flow = Flows.Custom,
                    CustomGrantTypeRestrictions = new List<string>
                    {
                        "custom"
                    },
                    
                    ScopeRestrictions = new List<string>
                    { 
                        "read",
                        "write",
                    },
                }
            };
        }
    }
}