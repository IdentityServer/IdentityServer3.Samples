using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace IdentityServerAspNet5
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new[]
            {
                StandardScopes.OpenId,
                StandardScopes.ProfileAlwaysInclude,
                StandardScopes.EmailAlwaysInclude,

                new Scope
                {
                    Name = "api1",
                    DisplayName = "API 1",
                    Type = ScopeType.Resource
                }
            };
        }
    }
}