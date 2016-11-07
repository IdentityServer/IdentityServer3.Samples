namespace AcceptanceTesting
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using FluentAssertions;
    using IdentityModel.Client;
    using Xbehave;

    public class JustLikeConsole
    {
        private static readonly string ServerUrl = "http://localhost:11111";
        private static readonly string IdentityServerUrl = "http://localhost:5000";

        private IDisposable webApp;
        private IDisposable identityServer;
        private HttpClient appClient;

        [Background]
        public void Background()
        {
            "establish server"._(() =>
            {
                this.identityServer = Microsoft.Owin.Hosting.WebApp.Start<IdSrv.Startup>(IdentityServerUrl);
                this.webApp = Microsoft.Owin.Hosting.WebApp.Start<Apis.Startup>(ServerUrl);
                this.appClient = new HttpClient { BaseAddress = new Uri(ServerUrl) };
            }).Teardown(() =>
            {
                this.appClient.Dispose();
                this.webApp.Dispose();
                this.identityServer.Dispose();
            });
        }

        [Scenario]
        public void SimplestScenario(HttpResponseMessage response)
        {
            var clientId = "silicon";
            var clientSecret = "F621F470-9731-4A25-80EF-67A6F7C5F4B8";
            var expectedJson = $"{{\"message\":\"OK computer\",\"client\":\"{clientId}\"}}";

            "get token"._(() =>
            {
                var client = new TokenClient(
                    $"{IdentityServerUrl}/connect/token",
                    clientId,
                    clientSecret);

                var token = client.RequestClientCredentialsAsync("api1").Result;
                this.appClient.SetBearerToken(token.AccessToken);
                Console.WriteLine(token.AccessToken);
            });

            "when calling a the service"._(()
                => response = this.appClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test"), CancellationToken.None).Result);

            "it should return status code 'OK'"._(()
                => response.StatusCode.Should().Be(HttpStatusCode.OK));

            "it should equal expected json"._(()
                => response.Content.ReadAsStringAsync().Result.Should().Be(expectedJson));
        }
    }
}
