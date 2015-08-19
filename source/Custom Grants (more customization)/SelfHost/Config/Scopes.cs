using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdentityServer3.Host.Config
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