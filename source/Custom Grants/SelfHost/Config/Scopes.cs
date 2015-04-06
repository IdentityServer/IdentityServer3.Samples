using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace IdentityServer.Host.Config
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new[]
                {
                    new Scope
                    {
                        Name = "read",
                        DisplayName = "Read data",
                        Type = ScopeType.Resource,
                        Emphasize = false
                    },
                };
        }
    }
}