namespace IdentityServer.Config
{
    using IdentityServer3.Core;
    using IdentityServer3.Core.Services.InMemory;
    using System.Collections.Generic;
    using System.Security.Claims;

    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Username = "bob",
                    Password = "secret",
                    Subject = "1",

                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "bob.smith@email.com")
                    }
                }
            };
        }
    }
}