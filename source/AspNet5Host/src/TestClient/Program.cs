using System;
using IdentityModel.Client;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TestClient
{
    public class Program
    {
        public void Main(string[] args)
        {
            var response = GetToken();

            var client = new HttpClient();
            client.SetBearerToken(response.AccessToken);

            var result = client.GetStringAsync("http://localhost:2025/claims").Result;
            Console.WriteLine(JArray.Parse(result));
        }

        TokenResponse GetToken()
        {
            var client = new TokenClient(
                "https://localhost:44300/connect/token",
                "test",
                "secret");

            return client.RequestClientCredentialsAsync("api1").Result;
        }
    }
}