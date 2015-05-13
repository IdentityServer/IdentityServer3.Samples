using Newtonsoft.Json.Linq;
using Sample;
using SampleWCFApiHost;
using SampleWCFApiHost.CustomToken;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityModel.Extensions;


namespace WCF_Console_Resource_Owner_Flow
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = RequestToken();
            ShowResponse(response);
            
            System.Threading.Thread.Sleep(2000);

            CallServiceCustomToken(response.AccessToken);
        }

        static Binding CreateCustomTokenBinding()
        {
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();

            // the message security binding element will be configured to require a credit card
            // token that is encrypted with the service's certificate 
            SymmetricSecurityBindingElement messageSecurity = new SymmetricSecurityBindingElement();
            messageSecurity.EndpointSupportingTokenParameters.SignedEncrypted.Add(new CustomTokenParameters());

            X509SecurityTokenParameters x509ProtectionParameters = new X509SecurityTokenParameters();
            x509ProtectionParameters.InclusionMode = SecurityTokenInclusionMode.Never;
            messageSecurity.ProtectionTokenParameters = x509ProtectionParameters;

            return new CustomBinding(messageSecurity, httpTransport);
        }

        static void CallServiceCustomToken(string token)
        {
            Binding customTokenBinding = CreateCustomTokenBinding();

            customTokenBinding.ReceiveTimeout = new TimeSpan(12, 0, 0);
            customTokenBinding.SendTimeout = new TimeSpan(12, 0, 0);
            customTokenBinding.OpenTimeout = new TimeSpan(12, 0, 0);
            customTokenBinding.CloseTimeout = new TimeSpan(12, 0, 0);

            var endPointIdentity = new DnsEndpointIdentity("idsrv3test");

            var serviceAddress = new EndpointAddress(new Uri("http://localhost:2729/Service1.svc"), endPointIdentity);

            // Create a client with given client endpoint configuration
            var channelFactory = new ChannelFactory<IService1>(customTokenBinding, serviceAddress);

            // configure the credit card credentials on the channel factory 
            CustomTokenClientCredentials credentials = new CustomTokenClientCredentials(token);

            // configure the service certificate on the credentials
            credentials.ServiceCertificate.DefaultCertificate = LoadCertificate();

            // replace ClientCredentials with CreditCardClientCredentials
            channelFactory.Endpoint.Behaviors.Remove(typeof(ClientCredentials));
            channelFactory.Endpoint.Behaviors.Add(credentials);

            var client = channelFactory.CreateChannel();

            var response = client.GetIdentityData();

            ((IChannel)client).Close();
            channelFactory.Close();

            "\n\nService claims:\n".ConsoleGreen();
            Console.WriteLine(response);
            Console.ReadLine();
        }

        static TokenResponse RequestToken()
        {
            var client = new OAuth2Client(
                new Uri(Sample.Constants.TokenEndpoint),
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

        static void ShowResponse(TokenResponse response)
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

        static SecurityToken ConvertToken(TokenResponse response)
        {
            if (response.Error == null)
            {
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement("o", "BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                element.SetAttribute("ValueType", "urn:ietf:params:oauth:token-type:jwt");
                element.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
                element.SetAttribute("Id", Guid.NewGuid().ToString());
                UTF8Encoding encoding = new UTF8Encoding();
                element.InnerText = Convert.ToBase64String(encoding.GetBytes(response.AccessToken));

                GenericXmlSecurityToken token = new GenericXmlSecurityToken(
                    element,
                    null,
                    DateTime.Now.AddDays(-10),
                    DateTime.Now.AddDays(10),
                    null,
                    null,
                    null);


                return token;
            }

            return null;
        }

        static X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\config\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }




        //static void CallServiceFederatedJWTToken(SecurityToken token)
        //{
        //    var binding = CreateFederatedBindingJWTToken();

        //    //var endPointIdentity = new DnsEndpointIdentity("idsrv3test");

        //    //var endpointAddress = new EndpointAddress(new Uri("http://localhost:2729/Service1.svc"), endPointIdentity);
        //    var endpointAddress = new EndpointAddress(new Uri("https://localhost:2728/Service1.svc"));

        //    var factory = new ChannelFactory<IService1>(binding, endpointAddress);

        //    factory.Credentials.SupportInteractive = false;
        //    factory.Credentials.UseIdentityConfiguration = true;
        //    //factory.Credentials.ServiceCertificate.DefaultCertificate = LoadCertificate();
        //    //factory.Credentials.ClientCertificate.Certificate = LoadCertificate();

        //    var proxy = factory.CreateChannelWithIssuedToken(token);

        //    var response = proxy.GetIdentityData();

        //    "\n\nService claims:".ConsoleGreen();
        //    Console.WriteLine(response);
        //    Console.ReadLine();
        //}

        //static Binding CreateBindingJWTToken()
        //{
        //    HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            
        //    TransportSecurityBindingElement messageSecurity = new TransportSecurityBindingElement();

        //    messageSecurity.AllowInsecureTransport = true;

        //    IssuedSecurityTokenParameters issuedTokenParameters = new IssuedSecurityTokenParameters();

        //    issuedTokenParameters.TokenType = "urn:ietf:params:oauth:token-type:jwt";

        //    messageSecurity.EndpointSupportingTokenParameters.Signed.Add(issuedTokenParameters);

        //    TextMessageEncodingBindingElement encodingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8);

        //    return new CustomBinding(messageSecurity, encodingElement, httpTransport);
        //}

        //static Binding CreateBindingJWTTokenS()
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

        //static WS2007FederationHttpBinding CreateFederatedBindingJWTToken()
        //{
        //    WS2007FederationHttpBinding fedBinding = new WS2007FederationHttpBinding();

        //    fedBinding.Security = new WSFederationHttpSecurity();
        //    fedBinding.Security.Mode = WSFederationHttpSecurityMode.TransportWithMessageCredential;
        //    fedBinding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;
        //    fedBinding.Security.Message.EstablishSecurityContext = false;
        //    fedBinding.Security.Message.IssuedTokenType = "urn:ietf:params:oauth:token-type:jwt";

        //    return fedBinding;
        //}


        //private static void CallServiceByReference(SecurityToken securityToken)
        //{
        //    ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();

        //    client.ChannelFactory.CreateChannelWithIssuedToken(securityToken);

        //    var response = client.GetIdentityData();

        //    "\n\nService claims:".ConsoleGreen();
        //    Console.WriteLine(response);
        //    Console.ReadLine();
        //}

        //static void CallServiceJWTToken(SecurityToken token)
        //{
        //    var binding = CreateBindingJWTToken();

        //    var endPointIdentity = new DnsEndpointIdentity("idsrv3test");

        //    var endpointAddress = new EndpointAddress(new Uri("http://localhost:2729/Service1.svc"), endPointIdentity);

        //    var factory = new ChannelFactory<IService1>(binding, endpointAddress);

        //    var proxy = factory.CreateChannelWithIssuedToken(token);

        //    var response = proxy.GetIdentityData();

        //    "\n\nService claims:".ConsoleGreen();
        //    Console.WriteLine(response);
        //    Console.ReadLine();
        //}

        //static void CallServiceJWTTokenS(SecurityToken token)
        //{
        //    var binding = CreateBindingJWTTokenS();

        //    var endpointAddress = new EndpointAddress(new Uri("https://localhost:2728/Service1.svc"));

        //    var factory = new ChannelFactory<IService1>(binding, endpointAddress);

        //    factory.Credentials.ClientCertificate.Certificate = LoadCertificate();
        //    factory.Credentials.ServiceCertificate.DefaultCertificate = LoadCertificate();

        //    factory.Credentials.SupportInteractive = false;
        //    factory.Credentials.UseIdentityConfiguration = false;

        //    //var proxy = factory.CreateChannelWithIssuedToken(token);
        //    var proxy = factory.CreateChannel();

        //    var response = proxy.GetIdentityData();

        //    "\n\nService claims:".ConsoleGreen();
        //    Console.WriteLine(response);
        //    Console.ReadLine();
        //}

        //static void CallServiceCustomBiding(SecurityToken token)
        //{
        //    var binding = CreateBindingJWTToken();

        //    var endPointIdentity = new DnsEndpointIdentity("idsrv3test");

        //    var endpointAddress = new EndpointAddress(new Uri("http://localhost:2729/Service1.svc"), endPointIdentity);

        //    binding.SendTimeout = new TimeSpan(12, 0, 0);
        //    binding.ReceiveTimeout = new TimeSpan(12, 0, 0);

        //    var factory = new ChannelFactory<IService1>(binding, endpointAddress);

        //    factory.Credentials.ClientCertificate.Certificate = LoadCertificate();
        //    factory.Credentials.ServiceCertificate.DefaultCertificate = LoadCertificate();

        //    factory.Credentials.SupportInteractive = false;

        //    var proxy = factory.CreateChannelWithIssuedToken(token);                      

        //    CommunicationState commState =  ((IClientChannel)proxy).State;
        //    Console.WriteLine(commState);
        //    ((IClientChannel)proxy).Open();

        //    System.Threading.Thread.Sleep(500);

        //    commState = ((IClientChannel)proxy).State;
        //    Console.WriteLine(commState);

        //    var response = proxy.GetIdentityData();

        //    "\n\nService claims:".ConsoleGreen();
        //    Console.WriteLine(response);
        //    Console.ReadLine();
        //}

        //static void CallServiceMultiFactorAuthenticationBinding(SecurityToken token, EndpointAddress serviceAddress)
        //{

        //    var multipleTokensBinding = CreateMultiFactorAuthenticationBinding();

        //    // Create a proxy with the previously create binding and endpoint address
        //    var channelFactory = new ChannelFactory<IService1>(multipleTokensBinding, serviceAddress);

        //    // configure the username credentials, the client certificate and the server certificate on the channel factory 
        //    channelFactory.Credentials.UserName.UserName = "xnavarro";
        //    channelFactory.Credentials.UserName.Password = "231*op*69";

        //    channelFactory.Credentials.ClientCertificate.Certificate = LoadCertificate();
        //    channelFactory.Credentials.ServiceCertificate.DefaultCertificate = LoadCertificate();

        //    var client = channelFactory.CreateChannelWithIssuedToken(token);

        //    var response = client.GetIdentityData();
        //    ((IChannel)client).Close();
        //    channelFactory.Close();

        //    "\n\nService claims:".ConsoleGreen();
        //    Console.WriteLine(response);
        //    Console.ReadLine();
        //}

        //static Binding CreateBindingCustomBinding()
        //{
        //    System.ServiceModel.Channels.AsymmetricSecurityBindingElement asbe = new AsymmetricSecurityBindingElement();
        //    asbe.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12;

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

        //static Binding CreateMultiFactorAuthenticationBinding()
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
