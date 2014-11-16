using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Linq;
using Thinktecture.IdentityServer.v3.AccessTokenValidation;

namespace Owin
{
    public static class IdentityServerAccessTokenValidationAppBuilderExtensions
    {
        public static IAppBuilder UseIdentityServerBearerTokenAuthentication(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (options.ValidationType == ValidationType.Local)
            {
                app.UseLocalValidation(options);
            }
            else if (options.ValidationType == ValidationType.ValidationEndpoint)
            {
                app.UseValidationEndpoint(options);
            }

            if (options.RequiredScopes.Any())
            {
                app.Use<ScopeRequirementMiddleware>(options.RequiredScopes);
            }

            return app;
        }

        internal static void UseLocalValidation(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            var discoveryEndpoint = options.Authority;
            if (!discoveryEndpoint.EndsWith("/"))
            {
                discoveryEndpoint += "/";
            }

            discoveryEndpoint += ".well-known/openid-configuration";
            
            var provider = new DiscoveryCachingSecurityTokenProvider(
                discoveryEndpoint,
                options.BackchannelCertificateValidator,
                options.BackchannelHttpHandler);

            JwtFormat jwtFormat = null;
            if (options.TokenValidationParameters != null)
            {
                jwtFormat = new JwtFormat(options.TokenValidationParameters, provider);
            }
            else
            {
                jwtFormat = new JwtFormat(provider.Audience, provider);
            }

            if (options.TokenHandler != null)
            {
                jwtFormat.TokenHandler = options.TokenHandler;
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = provider.Audience,
                Provider = options.Provider,
                AccessTokenFormat = jwtFormat,
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);
        }

        internal static void UseValidationEndpoint(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenProvider = new ValidationEndpointTokenProvider(options)
            });
        }


























        //internal static void UseLocal(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        //{





        //    //if (!string.IsNullOrWhiteSpace(options.Authority))
        //    //{
        //    //    return app.UseJwtDiscovery(options);
        //    //}
        //    //else
        //    //{
        //    //    return app.ConfigureJwtMiddleware(options.IdentityServerName, options.SigningCertificates, options.AuthenticationType);
        //    //}
        //}









        //private static IAppBuilder UseJwtDiscovery(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        //{
        //    var authority = options.Authority;

        //    if (!authority.EndsWith("/"))
        //    {
        //        authority += "/";
        //    }

        //    authority += ".well-known/openid-configuration";
        //    var configuration = new ConfigurationManager<OpenIdConnectConfiguration>(authority);

        //    var result = configuration.GetConfigurationAsync().Result;
        //    var certs = from key in result.JsonWebKeySet.Keys
        //                select new X509Certificate2(Convert.FromBase64String(key.X5c.First()));

        //    return app.ConfigureJwtMiddleware(result.Issuer, certs, options.AuthenticationType);
        //}

        //internal static IAppBuilder ConfigureJwtMiddleware(this IAppBuilder app, string issuerName, IEnumerable<X509Certificate2> signingCertificates, string authenticationType)
        //{
        //    if (string.IsNullOrWhiteSpace(issuerName)) throw new ArgumentNullException("issuerName");
        //    if (!signingCertificates.Any()) throw new ArgumentNullException("signingCertificate");

        //    var audience = issuerName;

        //    if (!audience.EndsWith("/"))
        //    {
        //        audience += "/";
        //    }

        //    audience += "resources";

        //    var providers = new List<X509CertificateSecurityTokenProvider>();
        //    foreach (var cert in signingCertificates)
        //    {
        //        providers.Add(new X509CertificateSecurityTokenProvider(issuerName, cert));
        //    }

        //    app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
        //    {
        //        AuthenticationType = authenticationType,

        //        AllowedAudiences = new[] { audience },
        //        IssuerSecurityTokenProviders = providers
        //    });

        //    return app;
        //}
    }
}