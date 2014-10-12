using System;
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

                    RedirectUris = new List<Uri>
                    {
                        new Uri("https://localhost:44319/")
                    }
                }
            };
        }
    }
}