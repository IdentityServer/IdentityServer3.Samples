using IdentityServer3.Core.Services.InMemory;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Username = "bob",
                    Password = "secret",
                    Subject = "bob",
                    Claims = new Claim[]
                    {
                        new Claim("name", "Bob Booker"),
                        new Claim("given_name", "Bob"),
                        new Claim("family_name", "Booker"),
                        new Claim("email", "BobBooker@email.com"),
                        new Claim("role", "Admin"),
                        new Claim("role", "Geek"),
                        new Claim("website", "http://bob.com")
                    }
                },
                new InMemoryUser
                {
                    Username = "alice",
                    Password = "secret",
                    Subject = "alice",
                    Claims = new Claim[]
                    {
                        new Claim("name", "Alice Smith"),
                        new Claim("given_name", "Alice"),
                        new Claim("family_name", "Smith"),
                        new Claim("email", "AliceSmith@email.com"),
                        new Claim("role", "Admin"),
                        new Claim("role", "Geek"),
                        new Claim("website", "http://alice.com")
                    }
                }
            };
        }
    }
}