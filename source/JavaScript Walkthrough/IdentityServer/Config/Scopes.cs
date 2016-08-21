namespace IdentityServer.Config
{
    using IdentityServer3.Core.Models;
    using System.Collections.Generic;

    public static class Scopes
    {
        public static List<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,

                new Scope
                {
                    Name = "api",

                    DisplayName = "Access to API",
                    Description = "This will grant you access to the API",

                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("api-secret".Sha256())
                    },

                    Type = ScopeType.Resource
                }
            };
        }
    }
}