using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace SampleApp.Config
{
    static class Users
    {
        public static IEnumerable<InMemoryUser> Get()
        {
            return new InMemoryUser[]
            {
                new InMemoryUser{Subject = "alice", Username = "alice", Password = "alice", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                    }
                },
                new InMemoryUser{Subject = "bob", Username = "bob", Password = "bob", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                    }
                },
            };
        }
    }
}