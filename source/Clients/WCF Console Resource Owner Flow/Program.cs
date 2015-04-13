using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityModel.Extensions;

namespace WCF_Console_Resource_Owner_Flow
{
    /// <summary>
    /// https://auth0.com/docs/wcf-tutorial
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var response = RequestToken();
            ShowResponse(response);

            Console.ReadLine();
            CallService(response.AccessToken);
        }

        static TokenResponse RequestToken()
        {
            var client = new OAuth2Client(
                new Uri(Constants.TokenEndpoint),
                "roclient",
                "secret");

            // idsrv supports additional non-standard parameters 
            // that get passed through to the user service
            var optional = new Dictionary<string, string>
            {
                { "acr_values", "tenant:custom_account_store1 foo bar quux" }
            };

            return client.RequestResourceOwnerPasswordAsync("bob", "bob", "read write", optional).Result;
        }

        static void CallService(string token)
        {
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

            client.ChannelFactory.Endpoint.Behaviors.Add(new AttachTokenEndpointBehavior(token));

            var response = client.GetIdentityDataAsync().Result;

            "\n\nService claims:".ConsoleGreen();
            Console.WriteLine(response);
            Console.ReadLine();
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
