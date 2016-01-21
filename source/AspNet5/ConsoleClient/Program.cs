using IdentityModel.Client;
using System;
using System.IdentityModel.Selectors;
using System.Net.Http;

namespace ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var tokenClient = new TokenClient(
				"http://localhost:18942/connect/token",
				"test",
				"secret");

			//This responds with the token for the "api" scope, based on the username/password above
			var response = tokenClient.RequestClientCredentialsAsync("api1").Result;

			//Test area to show api/values is protected
			//Should return that the request is unauthorized
			try
			{
				var unTokenedClient = new HttpClient();
				var unTokenedClientResponse = unTokenedClient.GetAsync("http://localhost:19806/api/values").Result;
				Console.WriteLine("Un-tokened response: {0}", unTokenedClientResponse.StatusCode);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception of: {0} while calling api without token.", ex.Message);
			}


			//Now we make the same request with the token received by the auth service.
			var client = new HttpClient();
			client.SetBearerToken(response.AccessToken);

			var apiResponse = client.GetAsync("http://localhost:19806/identity").Result;
			var callApiResponse = client.GetAsync("http://localhost:19806/api/values").Result;
			Console.WriteLine("Tokened response: {0}", callApiResponse.StatusCode);
			Console.WriteLine(callApiResponse.Content.ReadAsStringAsync().Result);
			Console.Read();
		}
	}
}