using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace IdSrv
{
    static class Scopes
    {
        public static List<Scope> Get()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "api1"
                }
            };
        }
    }
}