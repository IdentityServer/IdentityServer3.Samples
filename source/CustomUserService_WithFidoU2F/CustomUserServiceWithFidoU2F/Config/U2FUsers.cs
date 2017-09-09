using System.Collections.Generic;
using System.Linq;

namespace SampleApp.Config
{
    using System.Security.Claims;
    using IdentityServer3.Core;
    using SampleApp.Models;

    internal static class U2FUsers
    {
        public static List<U2FUser> Get()
        {
            var users = new List<U2FUser>
            {
                new U2FUser{Subject = "818727", Username = "alice", Password = "alice",
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.Name, "Alice Smith"),
                        new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                        new Claim(Constants.ClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(Constants.ClaimTypes.Role, "Admin"),
                        new Claim(Constants.ClaimTypes.Role, "Geek"),
                        new Claim(Constants.ClaimTypes.WebSite, "http://alice.com"),
                        new Claim(Constants.ClaimTypes.Address, @"{ ""street_address"": ""One Hacker Way"", ""locality"": ""Heidelberg"", ""postal_code"": 69118, ""country"": ""Germany"" }", Constants.ClaimValueTypes.Json)
                    }.ToList()
                },
                new U2FUser{Subject = "88421113", Username = "bob", Password = "bob",
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.Name, "Bob Smith"),
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(Constants.ClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(Constants.ClaimTypes.Role, "Developer"),
                        new Claim(Constants.ClaimTypes.Role, "Geek"),
                        new Claim(Constants.ClaimTypes.WebSite, "http://bob.com"),
                        new Claim(Constants.ClaimTypes.Address, @"{ ""street_address"": ""One Hacker Way"", ""locality"": ""Heidelberg"", ""postal_code"": 69118, ""country"": ""Germany"" }", Constants.ClaimValueTypes.Json)
                    }.ToList()
                },
            };

            return users;
        }
    }
}