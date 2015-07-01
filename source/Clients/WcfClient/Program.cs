using IdentityModel.Client;
using IdentityModel.Constants;
using IdentityModel.Extensions;
using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.ServiceModel;
using System.Xml.Linq;
using WcfService;

namespace WcfClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var jwt = GetJwt();
            var xmlToken = WrapJwt(jwt);

            var binding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            binding.HostNameComparisonMode = HostNameComparisonMode.Exact;
            binding.Security.Message.EstablishSecurityContext = false;
            binding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;

            var factory = new ChannelFactory<IService>(
                binding,
                new EndpointAddress("https://localhost:44335/token"));

            var channel = factory.CreateChannelWithIssuedToken(xmlToken);
            
            Console.WriteLine(channel.Ping());
        }

        static GenericXmlSecurityToken WrapJwt(string jwt)
        {
            var subject = new ClaimsIdentity("saml");
            subject.AddClaim(new Claim("jwt", jwt));

            var descriptor = new SecurityTokenDescriptor
            {
                TokenType = TokenTypes.Saml2TokenProfile11,
                TokenIssuerName = "urn:wrappedjwt",
                Subject = subject
            };

            var handler = new Saml2SecurityTokenHandler();
            var token = handler.CreateToken(descriptor);

            var xmlToken = new GenericXmlSecurityToken(
                XElement.Parse(token.ToTokenXmlString()).ToXmlElement(),
                null,
                DateTime.Now,
                DateTime.Now.AddHours(1),
                null,
                null,
                null);

            return xmlToken;
        }

        static string GetJwt()
        {
            var oauth2Client = new OAuth2Client(
                new Uri("https://localhost:44333/core/connect/token"),
                "roclient",
                "secret");

            var tokenResponse =
                oauth2Client.RequestResourceOwnerPasswordAsync("bob", "bob", "write").Result;

            return tokenResponse.AccessToken;
        }
    }
}
