using IdentityModel.Client;
using System;
using System.Text;
using IdentityModel.Extensions;
using Sample;
using Newtonsoft.Json.Linq;
using IdentityModel;

namespace ConsoleResourceOwnerWithUserInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = RequestToken();
            ShowResponse(response);

            GetClaims(response.AccessToken);
        }

        static TokenResponse RequestToken()
        {
            var client = new TokenClient(
                Constants.TokenEndpoint,
                "roclient",
                "secret");

            return client.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email").Result;
        }

        static void GetClaims(string token)
        {
            var client = new UserInfoClient(
                new Uri(Constants.UserInfoEndpoint),
                token);

            var response = client.GetAsync().Result;
            var identity = response.GetClaimsIdentity();

            "\n\nUser claims:".ConsoleGreen();
            foreach (var claim in identity.Claims)
            {
                Console.WriteLine("{0}\n {1}", claim.Type, claim.Value);
            }
        }

        private static void ShowResponse(TokenResponse response)
        {
            if (!response.IsError)
            {
                "Token response:".ConsoleGreen();
                Console.WriteLine(response.Json);

                if (response.AccessToken.Contains("."))
                {
                    "\nAccess Token (decoded):".ConsoleGreen();

                    var parts = response.AccessToken.Split('.');
                    var header = parts[0];
                    var claims = parts[1];

                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
                }
            }
            else
            {
                if (response.IsHttpError)
                {
                    "HTTP error: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorStatusCode);
                    "HTTP error reason: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorReason);
                }
                else
                {
                    "Protocol error response:".ConsoleGreen();
                    Console.WriteLine(response.Json);
                }
            }
        }
    }
}