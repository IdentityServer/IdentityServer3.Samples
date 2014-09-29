using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Host.Config
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