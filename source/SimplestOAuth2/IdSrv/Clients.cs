using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace IdSrv
{
    static class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                // no human involved
                new Client
                {
                    ClientName = "Silicon-only Client",
                    ClientId = "silicon",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Reference,

                    Flow = Flows.ClientCredentials,
                    ClientSecrets = new List<ClientSecret>
                    {
                        new ClientSecret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
                    }
                }
            };
        }
    }
}