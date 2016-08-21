namespace IdentityServer.Config
{
    using IdentityServer3.Core.Models;
    using System.Collections.Generic;

    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client
                {
                    Enabled = true,
                    ClientName = "JS Client",
                    ClientId = "js",
                    Flow = Flows.Implicit,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:56668/popup.html",
                        "http://localhost:56668/silent-renew.html"
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:56668/index.html"
                    },

                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:56668"
                    },

                    AllowAccessToAllScopes = true,
                    AccessTokenLifetime = 60
                }
            };
        }
    }
}