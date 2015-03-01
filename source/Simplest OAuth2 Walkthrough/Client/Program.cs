using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Client;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // Disable SSL Validation for testing purposes
            // Credits: http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using
            ServicePointManager.ServerCertificateValidationCallback =
                delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
            
            var response = GetClientToken();
            CallApi(response);

            response = GetUserToken();
            CallApi(response);
        }

        static void CallApi(TokenResponse response)
        {
            var client = new HttpClient();
            client.SetBearerToken(response.AccessToken);

            Console.WriteLine(client.GetStringAsync("http://localhost:14869/test").Result);
        }

        static TokenResponse GetClientToken()
        {
            var client = new OAuth2Client(
                new Uri("https://localhost:44333/connect/token"),
                "silicon",
                "F621F470-9731-4A25-80EF-67A6F7C5F4B8");

            return client.RequestClientCredentialsAsync("api1").Result;
        }

        static TokenResponse GetUserToken()
        {
            var client = new OAuth2Client(
                new Uri("https://localhost:44333/connect/token"),
                "carbon",
                "21B5F798-BE55-42BC-8AA8-0025B903DC3B");

            return client.RequestResourceOwnerPasswordAsync("bob", "secret", "api1").Result;
        }
    }
}
