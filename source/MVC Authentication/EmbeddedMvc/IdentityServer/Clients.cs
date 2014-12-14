using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace EmbeddedMvc.IdentityServer
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client 
                {
                    Enabled = true,
                    ClientName = "MVC Client",
                    ClientId = "mvc",
                    Flow = Flows.Hybrid,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44319/"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "https://localhost:44319/"
                    }
                },
                new Client
                {
                    Enabled = true,
                    ClientName = "MVC Client (service communication)",
                    ClientId = "mvc_service",
                    ClientSecret = "secret",
                    Flow = Flows.ClientCredentials
                }
            };
        }
    }
}