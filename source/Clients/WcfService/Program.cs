using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace WcfService
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(
                typeof(Service), 
                new Uri("https://localhost:44335"));

            host.Credentials.IdentityConfiguration = CreateIdentityConfiguration();
            host.Credentials.UseIdentityConfiguration = true;

            var authz = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
            authz.PrincipalPermissionMode = PrincipalPermissionMode.Always;
            
            host.AddServiceEndpoint(typeof(IService), CreateBinding(), "token");

            host.Open();

            Console.WriteLine("server running...");
            Console.ReadLine();

            host.Close();
        }

        static IdentityConfiguration CreateIdentityConfiguration()
        {
            var identityConfiguration = new IdentityConfiguration();

            identityConfiguration.SecurityTokenHandlers.Clear();
            identityConfiguration.SecurityTokenHandlers.Add(new IdentityServerWrappedJwtHandler("https://localhost:44333/core", "write"));
            identityConfiguration.ClaimsAuthorizationManager = new RequireAuthenticationAuthorizationManager();

            return identityConfiguration;
        }

        static Binding CreateBinding()
        {
            var binding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);

            // only for testing on localhost
            binding.HostNameComparisonMode = HostNameComparisonMode.Exact;

            binding.Security.Message.EstablishSecurityContext = false;
            binding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;

            return binding;
        }
    }
}