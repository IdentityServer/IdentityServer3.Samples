using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = GetToken();
            CallApi(response);

            Console.Read();
        }

        static void CallApi(TokenResponse response)
        {
            var client = new HttpClient();
            client.SetBearerToken(response.AccessToken);

            Console.WriteLine(client.GetStringAsync("http://localhost:44334/test").Result);
        }

        static TokenResponse GetToken()
        {
            var client = new TokenClient(
                "http://localhost:44333/connect/token",
                "ConsoleApplication",
                "F621F470-9731-4A25-80EF-67A6F7C5F4B8");

            return client.RequestResourceOwnerPasswordAsync("bob", "secret", "WebApi1").Result;
        }
    }
}
