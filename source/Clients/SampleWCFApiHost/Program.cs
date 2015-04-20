using SampleWCFApiHost.Config;
using System;
using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading.Tasks;

namespace SampleWCFApiHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //EndpointAddress addressHTTPS = new EndpointAddress("https://localhost:2728/Service1.svc");
            EndpointAddress addressHTTP = new EndpointAddress("http://localhost:2729/Service1.svc");

            using (var host = new ServiceHost(typeof(Service1), addressHTTP.Uri ) )
            {
                ConfigureForJWTToken(host, addressHTTP.Uri);

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

        private static void ConfigureForJWTToken(ServiceHost host, Uri address)
        {
            // Extract the ServiceCredentials behavior or create one.
            ServiceCredentials serviceCredentials = host.Description.Behaviors.Find<ServiceCredentials>();
            if (serviceCredentials == null)
            {
                serviceCredentials = new ServiceCredentials();
                host.Description.Behaviors.Add(serviceCredentials);
            }

            // Set the service certificate.
            host.Credentials.ServiceCertificate.Certificate = Certificate.Get();
            host.Credentials.UseIdentityConfiguration = true;

            IdentityConfiguration idConfiguration = new IdentityConfiguration();

            idConfiguration.SecurityTokenHandlers.Add(new CustomJwtSecurityTokenHandler.CustomJwtSecurityTokenHandler());            

            host.Credentials.IdentityConfiguration = idConfiguration;

            // Create the custom binding and add an endpoint to the service.
            Binding customTokenBinging = CreateBindingForJWTToken();
            host.AddServiceEndpoint(typeof(IService1), customTokenBinging, address);
        }

        static Binding CreateBindingForJWTToken()
        {
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();

            TransportSecurityBindingElement messageSecurity = new TransportSecurityBindingElement();

            messageSecurity.AllowInsecureTransport = true;
            messageSecurity.DefaultAlgorithmSuite = SecurityAlgorithmSuite.Default;
            messageSecurity.IncludeTimestamp = true;
            
            IssuedSecurityTokenParameters issuerTokenParameters = new IssuedSecurityTokenParameters();

            issuerTokenParameters.TokenType = "urn:ietf:params:oauth:token-type:jwt";

            messageSecurity.EndpointSupportingTokenParameters.Signed.Add(issuerTokenParameters);

            TextMessageEncodingBindingElement encodingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8);

            return new CustomBinding(messageSecurity, encodingElement, httpTransport);
        }

        //static CustomBinding CreateBindingCustomBinding()
        //{
        //    System.ServiceModel.Channels.AsymmetricSecurityBindingElement asbe = new AsymmetricSecurityBindingElement();
        //    asbe.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12;

        //    //asbe.InitiatorTokenParameters = new System.ServiceModel.Security.Tokens.X509SecurityTokenParameters { InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient };
        //    //asbe.RecipientTokenParameters = new System.ServiceModel.Security.Tokens.X509SecurityTokenParameters { InclusionMode = SecurityTokenInclusionMode.Never };
        //    asbe.InitiatorTokenParameters = new System.ServiceModel.Security.Tokens.IssuedSecurityTokenParameters { InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient };
        //    asbe.RecipientTokenParameters = new System.ServiceModel.Security.Tokens.IssuedSecurityTokenParameters { InclusionMode = SecurityTokenInclusionMode.Never };
        //    asbe.MessageProtectionOrder = System.ServiceModel.Security.MessageProtectionOrder.SignBeforeEncrypt;

        //    asbe.SecurityHeaderLayout = SecurityHeaderLayout.Strict;
        //    asbe.EnableUnsecuredResponse = true;
        //    asbe.IncludeTimestamp = false;
        //    asbe.SetKeyDerivation(false);
        //    asbe.DefaultAlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Basic128Rsa15;
        //    asbe.EndpointSupportingTokenParameters.Signed.Add(new IssuedSecurityTokenParameters());

        //    CustomBinding myBinding = new CustomBinding();
        //    myBinding.Elements.Add(asbe);
        //    myBinding.Elements.Add(new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8));

        //    HttpTransportBindingElement httpsBindingElement = new HttpTransportBindingElement();
        //    myBinding.Elements.Add(httpsBindingElement);

        //    return myBinding;
        //}

        //private static Binding CreateMultiFactorAuthenticationBinding()
        //{
        //    HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();

        //    // the message security binding element will be configured to require 2 tokens:
        //    // 1) A username-password encrypted with the service token
        //    // 2) A client certificate used to sign the message

        //    // Instantiate a binding element that will require the username/password token in the message (encrypted with the server cert)
        //    //SymmetricSecurityBindingElement messageSecurity = SecurityBindingElement.CreateUserNameForCertificateBindingElement();


        //    IssuedSecurityTokenParameters tokenParams = new IssuedSecurityTokenParameters();
        //    tokenParams.DefaultMessageSecurityVersion = MessageSecurityVersion.Default;

        //    SymmetricSecurityBindingElement messageSecurity = SecurityBindingElement.CreateIssuedTokenBindingElement(tokenParams);

        //    // Create supporting token parameters for the client X509 certificate.
        //    X509SecurityTokenParameters clientX509SupportingTokenParameters = new X509SecurityTokenParameters();
        //    // Specify that the supporting token is passed in message send by the client to the service
        //    clientX509SupportingTokenParameters.InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
        //    // Turn off derived keys
        //    clientX509SupportingTokenParameters.RequireDerivedKeys = false;
        //    // Augment the binding element to require the client's X509 certificate as an endorsing token in the message
        //    messageSecurity.EndpointSupportingTokenParameters.Endorsing.Add(clientX509SupportingTokenParameters);

        //    // Create a CustomBinding based on the constructed security binding element.
        //    return new CustomBinding(messageSecurity, httpTransport);
        //}
    }
}
