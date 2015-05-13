using SampleWCFApiHost.Config;
using SampleWCFApiHost.CustomToken;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;

namespace SampleWCFApiHost
{
    class Program
    {
        static void Main(string[] args)
        {
            EndpointAddress addressHTTPS = new EndpointAddress("https://localhost:2728/Service1.svc");
            EndpointAddress addressHTTP = new EndpointAddress("http://localhost:2729/Service1.svc");

            using (var host = new ServiceHost(typeof(Service1), new Uri[] { addressHTTPS.Uri, addressHTTP.Uri }))
            {
                //ConfigureForJWTTokenS(host, addressHTTPS.Uri);
                ConfigureForCustomBindingToken(host, addressHTTP.Uri);

                //Adding metadata exchange endpoint
                Binding mexBinding = MetadataExchangeBindings.CreateMexHttpBinding();                
                host.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, "mex");                               

                host.Open();

                Console.WriteLine("The service is ready at {0}", addressHTTP.Uri);
                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();
                host.Close();
            }
        }

        private static void ConfigureForCustomBindingToken(ServiceHost host, Uri address)
        {
            // Create a service credentials and add it to the behaviors.
            CustomTokenServiceCredentials serviceCredentials = new CustomTokenServiceCredentials();

            serviceCredentials.ServiceCertificate.Certificate = Certificate.Get();
            host.Description.Behaviors.Remove((typeof(ServiceCredentials)));
            host.Description.Behaviors.Add(serviceCredentials);

            // Register a binding for the endpoint.
            Binding customBinding = CreateCustomTokenBinding();
            host.AddServiceEndpoint(typeof(IService1), customBinding, string.Empty);
        }

        static Binding CreateCustomTokenBinding()
        {
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();

            // the message security binding element will be configured to require an AccessToken
            // token that is encrypted with the service's certificate 
            SymmetricSecurityBindingElement messageSecurity = new SymmetricSecurityBindingElement();
            messageSecurity.EndpointSupportingTokenParameters.SignedEncrypted.Add(new CustomTokenParameters());

            X509SecurityTokenParameters x509ProtectionParameters = new X509SecurityTokenParameters();
            x509ProtectionParameters.InclusionMode = SecurityTokenInclusionMode.Never;
            messageSecurity.ProtectionTokenParameters = x509ProtectionParameters;

            return new CustomBinding(messageSecurity, httpTransport);
        }


        //private static void ConfigureForCustomBinding(ServiceHost host, Uri address)
        //{
        //    // Extract the ServiceCredentials behavior or create one.
        //    ServiceCredentials serviceCredentials = host.Description.Behaviors.Find<ServiceCredentials>();
        //    if (serviceCredentials == null)
        //    {
        //        serviceCredentials = new ServiceCredentials();
        //        host.Description.Behaviors.Add(serviceCredentials);
        //    }

        //    // Set the service certificate.
        //    serviceCredentials.ServiceCertificate.Certificate = Certificate.Get();
        //    serviceCredentials.ClientCertificate.Certificate = Certificate.Get();

        //    // Create the custom binding and add an endpoint to the service.
        //    Binding multipleTokensBinding = CreateBindingCustomBinding();
        //    host.AddServiceEndpoint(typeof(IService1), multipleTokensBinding, address);

        //    //host.Credentials.UseIdentityConfiguration = true;

        //    //IdentityConfiguration idConfig = new IdentityConfiguration();

        //    //idConfig.SecurityTokenHandlers.Add(new CustomJwtSecurityTokenHandler.CustomJwtSecurityTokenHandler());
            
        //    //host.Credentials.IdentityConfiguration = idConfig;
        //}

        //private static void ConfigureForMultiFactorAuthenticationBinding(ServiceHost host, Uri address)
        //{
        //    // Extract the ServiceCredentials behavior or create one.
        //    ServiceCredentials serviceCredentials = host.Description.Behaviors.Find<ServiceCredentials>();
        //    if (serviceCredentials == null)
        //    {
        //        serviceCredentials = new ServiceCredentials();
        //        host.Description.Behaviors.Add(serviceCredentials);
        //    }

        //    // Set the service certificate.
        //    serviceCredentials.ServiceCertificate.Certificate = Certificate.Get();

        //    /*
        //    Setting the CertificateValidationMode to PeerOrChainTrust means that if the certificate 
        //    is in the Trusted People store, then it is trusted without performing a
        //    validation of the certificate's issuer chain. This setting is used here for convenience so that the 
        //    sample can be run without having to have certificates issued by a certificate authority (CA).
        //    This setting is less secure than the default, ChainTrust. The security implications of this 
        //    setting should be carefully considered before using PeerOrChainTrust in production code. 
        //    */
        //    serviceCredentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;

        //    // Create the custom binding and add an endpoint to the service.
        //    Binding multipleTokensBinding = CreateMultiFactorAuthenticationBinding();
        //    host.AddServiceEndpoint(typeof(IService1), multipleTokensBinding, address);
            
        //}

        //private static void ConfigureForFederationJWTToken(ServiceHost host, Uri address)
        //{
        //    // Extract the ServiceCredentials behavior or create one.
        //    ServiceCredentials serviceCredentials = host.Description.Behaviors.Find<ServiceCredentials>();
        //    if (serviceCredentials == null)
        //    {
        //        serviceCredentials = new ServiceCredentials();
        //        host.Description.Behaviors.Add(serviceCredentials);
        //    }

        //    // Set the service certificate.
        //    host.Credentials.ServiceCertificate.Certificate = Certificate.Get();
        //    host.Credentials.UseIdentityConfiguration = true;

        //    IdentityConfiguration idConfiguration = new IdentityConfiguration();

        //    idConfiguration.SecurityTokenHandlers.Add(new CustomJwtSecurityTokenHandler.CustomJwtSecurityTokenHandler());

        //    host.Credentials.IdentityConfiguration = idConfiguration;

        //    // Create the custom binding and add an endpoint to the service.
        //    var federatedTokenBinging = CreateFederationBindingForJWTToken();
        //    host.AddServiceEndpoint(typeof(IService1), federatedTokenBinging, address);
        //}

        //private static void ConfigureForJWTToken(ServiceHost host, Uri address)
        //{
        //    // Extract the ServiceCredentials behavior or create one.
        //    ServiceCredentials serviceCredentials = host.Description.Behaviors.Find<ServiceCredentials>();
        //    if (serviceCredentials == null)
        //    {
        //        serviceCredentials = new ServiceCredentials();
        //        host.Description.Behaviors.Add(serviceCredentials);
        //    }

        //    // Set the service certificate.
        //    host.Credentials.UseIdentityConfiguration = true;

        //    IdentityConfiguration idConfiguration = new IdentityConfiguration();

        //    idConfiguration.SecurityTokenHandlers.Add(new CustomJwtSecurityTokenHandler.CustomJwtSecurityTokenHandler());            

        //    host.Credentials.IdentityConfiguration = idConfiguration;

        //    // Create the custom binding and add an endpoint to the service.
        //    Binding customTokenBinging = CreateBindingForJWTToken();
        //    host.AddServiceEndpoint(typeof(IService1), customTokenBinging, address);
        //}

        //private static void ConfigureForJWTTokenS(ServiceHost host, Uri address)
        //{
        //    // Extract the ServiceCredentials behavior or create one.
        //    ServiceCredentials serviceCredentials = host.Description.Behaviors.Find<ServiceCredentials>();
        //    if (serviceCredentials == null)
        //    {
        //        serviceCredentials = new ServiceCredentials();
        //        host.Description.Behaviors.Add(serviceCredentials);
        //    }

        //    // Set the service certificate.
        //    host.Credentials.ServiceCertificate.Certificate = Certificate.Get();
        //    host.Credentials.UseIdentityConfiguration = true;

        //    IdentityConfiguration idConfiguration = new IdentityConfiguration();

        //    idConfiguration.SecurityTokenHandlers.Add(new CustomJwtSecurityTokenHandler.CustomJwtSecurityTokenHandler());

        //    host.Credentials.IdentityConfiguration = idConfiguration;

        //    // Create the custom binding and add an endpoint to the service.
        //    Binding customTokenBinging = CreateBindingForJWTTokenS();

        //    host.AddServiceEndpoint(typeof(IService1), customTokenBinging, address);
        //}

        //static Binding CreateBindingForJWTToken()
        //{
        //    HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            
        //    TransportSecurityBindingElement messageSecurity = new TransportSecurityBindingElement();

        //    messageSecurity.AllowInsecureTransport = true;
            
        //    IssuedSecurityTokenParameters issuedTokenParameters = new IssuedSecurityTokenParameters();
                        
        //    issuedTokenParameters.TokenType = "urn:ietf:params:oauth:token-type:jwt";

        //    messageSecurity.EndpointSupportingTokenParameters.Signed.Add(issuedTokenParameters);

        //    TextMessageEncodingBindingElement encodingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8);

        //    var customBinding = new CustomBinding(messageSecurity, encodingElement, httpTransport);

        //    return customBinding;
        //}

        //static Binding CreateBindingForJWTTokenS()
        //{
        //    HttpsTransportBindingElement httpsTransport = new HttpsTransportBindingElement();

        //    SymmetricSecurityBindingElement messageSecurity = new SymmetricSecurityBindingElement();

        //    IssuedSecurityTokenParameters issuedTokenParameters = new IssuedSecurityTokenParameters();

        //    issuedTokenParameters.TokenType = "urn:ietf:params:oauth:token-type:jwt";

        //    messageSecurity.EndpointSupportingTokenParameters.Signed.Add(issuedTokenParameters);

        //    TextMessageEncodingBindingElement encodingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8);

        //    var customBinding = new CustomBinding(messageSecurity, encodingElement, httpsTransport);

        //    return customBinding;
        //}

        //static WS2007FederationHttpBinding CreateFederationBindingForJWTToken()
        //{
        //    WS2007FederationHttpBinding fedBinding = new WS2007FederationHttpBinding();

        //    fedBinding.Security = new WSFederationHttpSecurity();
        //    fedBinding.Security.Mode = WSFederationHttpSecurityMode.TransportWithMessageCredential;
        //    fedBinding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;
        //    fedBinding.Security.Message.EstablishSecurityContext = false;
        //    fedBinding.Security.Message.IssuedTokenType = "urn:ietf:params:oauth:token-type:jwt";

        //    return fedBinding;
        //}
    }
}
