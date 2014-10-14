using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace EmbeddedMvc.IdentityServer
{
    public static class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            var scopes = new List<Scope>
            {
                new Scope
                {
                    Enabled = true,
                    Name = "roles",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("role")
                    }
                },
                new Scope
                {
                    Enabled = true,
                    Name = "sampleApi",
                    Description = "Access to a sample API",
                    Type = ScopeType.Resource
                }
            };

            scopes.AddRange(Scope.StandardScopes);

            return scopes;
        }
    }
}