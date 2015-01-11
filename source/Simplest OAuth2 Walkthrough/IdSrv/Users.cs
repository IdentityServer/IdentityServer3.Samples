using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace IdSrv
{
    static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>();
        }
    }
}