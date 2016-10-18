using Microsoft.Owin;
using Owin;
using Configuration;
using IdentityServer3.Core.Configuration;
using Serilog;
using IdentityServer3.Host.Config;
using Kentor.AuthServices.Owin;
using Kentor.AuthServices.Configuration;
using System.IdentityModel.Metadata;
using Kentor.AuthServices;
using System;

[assembly: OwinStartup(typeof(WebHost.Startup))]

namespace WebHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(@"c:\logs\IdentityServer3_with_SAML2-P.txt")
                .CreateLogger();

            var factory = new IdentityServerServiceFactory()
                        .UseInMemoryUsers(Users.Get())
                        .UseInMemoryClients(Clients.Get())
                        .UseInMemoryScopes(Scopes.Get());

            var options = new IdentityServerOptions
            {
                SigningCertificate = Certificate.Load(),
                Factory = factory,
                AuthenticationOptions = new AuthenticationOptions
                {
                    IdentityProviders = ConfigureIdPs,
                    EnableAutoCallbackForFederatedSignout = true
                }
            };

            app.Map("/core", idsrvApp =>
            {
                idsrvApp.UseIdentityServer(options);
            });
        }

        public static void ConfigureIdPs(IAppBuilder app, string signInAsType)
        {
            var idpUrl = "http://stubidp.kentor.se/40407893-db90-457b-ae0b-c283ed41d9bc/Metadata";

            KentorAuthServicesAuthenticationOptions authServicesOptions = null;
            authServicesOptions = new KentorAuthServicesAuthenticationOptions(false)
            {
                SPOptions = new SPOptions
                {
                    AuthenticateRequestSigningBehavior = SigningBehavior.Never,
                    EntityId = new EntityId("urn:idsvr3")
                },
                SignInAsAuthenticationType = signInAsType,
                AuthenticationType = "saml2-p",
                Caption = "SAML2-P",
                Notifications = new KentorAuthServicesNotifications
                {
                    SelectIdentityProvider = (id, env) =>
                    {
                        var idp = new IdentityProvider(
                            new EntityId(idpUrl),
                            new SPOptions
                            {
                                AuthenticateRequestSigningBehavior = SigningBehavior.Never,
                                EntityId = new EntityId("urn:idsvr3")
                            }
                        )
                        {
                            AllowUnsolicitedAuthnResponse = true,
                            LoadMetadata = true,
                            MetadataLocation = idpUrl,
                        };
                        authServicesOptions.IdentityProviders.Add(idp);
                        return idp;
                    },
                }
            };

            //authServicesOptions.IdentityProviders.Add(new IdentityProvider(
            //    new EntityId(idp1),
            //    new SPOptions
            //    {
            //        AuthenticateRequestSigningBehavior = SigningBehavior.Never,
            //        EntityId = new EntityId("urn:idsvr3-idp1")
            //    }
            //)
            //{
            //    AllowUnsolicitedAuthnResponse = true,
            //    LoadMetadata = true,
            //    MetadataLocation = idp1,
            //});

            //var idp2 = "http://stubidp.kentor.se/2f95c724-93d1-4afc-8429-266db7ca4b7b/Metadata";
            //authServicesOptions.IdentityProviders.Add(new IdentityProvider(
            //    new EntityId(idp2),
            //    new SPOptions
            //    {
            //        AuthenticateRequestSigningBehavior = SigningBehavior.Never,
            //        EntityId = new EntityId("urn:idsvr3-idp2")
            //    }
            //)
            //{
            //    AllowUnsolicitedAuthnResponse = true,
            //    LoadMetadata = true,
            //    MetadataLocation = idp2,
            //});

            app.UseKentorAuthServicesAuthentication(authServicesOptions);
        }
    }
}