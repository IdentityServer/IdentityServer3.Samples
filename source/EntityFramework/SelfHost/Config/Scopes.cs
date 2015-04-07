using System.Collections.Generic;
using System.Linq;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;

namespace SelfHost.Config
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new Scope[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.OfflineAccess,
                new Scope
                {
                    Name = "read",
                    DisplayName = "Read data",
                    Type = ScopeType.Resource,
                    Emphasize = false,
                },
                new Scope
                {
                    Name = "write",
                    DisplayName = "Write data",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                },
             };
        }
    }
}