using IdentityModel.Client;
using Sample;
using System;
using System.Linq;

namespace ConsoleClientCredentialsIntrospection
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = RequestToken();
            Introspection(response.AccessToken);
        }

        static TokenResponse RequestToken()
        {
            var client = new TokenClient(
                Constants.TokenEndpoint,
                "clientcredentials.client",
                "secret");

            return client.RequestClientCredentialsAsync("read write").Result;
        }

        private static void Introspection(string accessToken)
        {
            var client = new IntrospectionClient(
                "https://localhost:44333/core/connect/introspect",
                "write",
                "secret");

            var request = new IntrospectionRequest
            {
                Token = accessToken
            };

            var result = client.SendAsync(request).Result;

            if (result.IsError)
            {
                Console.WriteLine(result.Error);
            }
            else
            {
                if (result.IsActive)
                {
                    result.Claims.ToList().ForEach(c => Console.WriteLine("{0}: {1}",
                        c.Item1, c.Item2));
                }
                else
                {
                    Console.WriteLine("token is not active");
                }
            }
        }
    }
}