using IdentityModel.Client;
using System;
using System.Net.Http;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var tokenClient = new TokenClient(
                "http://localhost:5000/connect/token",
                "test",
                "secret");

            var response = tokenClient.RequestClientCredentialsAsync("api1").Result;

            var client = new HttpClient();
            client.SetBearerToken(response.AccessToken);

            var apiResponse = client.GetAsync("http://localhost:19806/identity").Result;
            Console.WriteLine(apiResponse.StatusCode);
        }
    }
}