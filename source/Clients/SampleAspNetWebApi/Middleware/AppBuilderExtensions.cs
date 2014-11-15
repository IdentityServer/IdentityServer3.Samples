using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.v3.AccessTokenValidation;

namespace Owin
{
    public static class IdentityServerAccessTokenValidationAppBuilderExtensions
    {
        public static IAppBuilder UseIdentityServerBearerTokenAuthentication(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            if (options.TokenType == ValidationType.Local)
            {
                app.UseJwt(options);
            }
            else if (options.TokenType == ValidationType.ValidationEndpoint)
            {
                app.UseValidationEndpoint(options);
            }

            if (options.RequiredScopes.Any())
            {
                // add scope mw
            }

            return app;
        }

        internal static IAppBuilder UseValidationEndpoint(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenProvider = new ValidationEndpointTokenProvider(options)
            });

            return app;
        }

        internal static IAppBuilder UseJwt(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Authority))
            {
                return app.UseJwtDiscovery(options);
            }
            else
            {
                return app.ConfigureJwtMiddleware(options.IdentityServerName, options.SigningCertificates, options.AuthenticationType);
            }
        }

        private static IAppBuilder UseJwtDiscovery(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            var authority = options.Authority;

            if (!authority.EndsWith("/"))
            {
                authority += "/";
            }

            authority += ".well-known/openid-configuration";
            var configuration = new ConfigurationManager<OpenIdConnectConfiguration>(authority);

            var result = configuration.GetConfigurationAsync().Result;
            var certs = from key in result.JsonWebKeySet.Keys
                        select new X509Certificate2(Convert.FromBase64String(key.X5c.First()));

            return app.ConfigureJwtMiddleware(result.Issuer, certs, options.AuthenticationType);
        }

        internal static IAppBuilder ConfigureJwtMiddleware(this IAppBuilder app, string issuerName, IEnumerable<X509Certificate2> signingCertificates, string authenticationType)
        {
            if (string.IsNullOrWhiteSpace(issuerName)) throw new ArgumentNullException("issuerName");
            if (!signingCertificates.Any()) throw new ArgumentNullException("signingCertificate");

            var audience = issuerName;

            if (!audience.EndsWith("/"))
            {
                audience += "/";
            }

            audience += "resources";

            var providers = new List<X509CertificateSecurityTokenProvider>();
            foreach (var cert in signingCertificates)
            {
                providers.Add(new X509CertificateSecurityTokenProvider(issuerName, cert));
            }

            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                AuthenticationType = authenticationType,

                AllowedAudiences = new[] { audience },
                IssuerSecurityTokenProviders = providers
            });

            return app;
        }
    }
}