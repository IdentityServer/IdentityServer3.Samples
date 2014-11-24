/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    internal class DiscoveryCachingSecurityTokenProvider : IIssuerSecurityTokenProvider
    {
        private readonly TimeSpan _refreshInterval = new TimeSpan(1, 0, 0, 0);
        private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim();
        private readonly string _discoveryEndpoint;
        private readonly HttpMessageHandler _backchannelHttpHandler;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        
        private DateTimeOffset _syncAfter = new DateTimeOffset(new DateTime(2001, 1, 1));
        private string _issuer;
        private IEnumerable<SecurityToken> _tokens;

        public DiscoveryCachingSecurityTokenProvider(string discoveryEndpoint, ICertificateValidator backchannelCertificateValidator, HttpMessageHandler backchannelHttpHandler)
        {
            _discoveryEndpoint = discoveryEndpoint;
            _backchannelHttpHandler = backchannelHttpHandler ?? new WebRequestHandler();

            if (backchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = _backchannelHttpHandler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException("Invalid certificate validator");
                }
                webRequestHandler.ServerCertificateValidationCallback = backchannelCertificateValidator.Validate;
            }

            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(_discoveryEndpoint, new HttpClient(_backchannelHttpHandler));

            RetrieveMetadata();
        }

        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        public string Issuer
        {
            get
            {
                RetrieveMetadata();
                _synclock.EnterReadLock();
                try
                {
                    return _issuer;
                }
                finally
                {
                    _synclock.ExitReadLock();
                }
            }
        }

        /// <value>
        /// The identity server default audience
        /// </value>
        public string Audience
        {
            get
            {
                RetrieveMetadata();
                _synclock.EnterReadLock();
                try
                {
                    var issuer = _issuer;

                    if (!issuer.EndsWith("/"))
                    {
                        issuer += "/";
                    }

                    return issuer += "resources";
                }
                finally
                {
                    _synclock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <value>
        /// All known security tokens.
        /// </value>
        public IEnumerable<SecurityToken> SecurityTokens
        {
            get
            {
                RetrieveMetadata();
                _synclock.EnterReadLock();
                try
                {
                    return _tokens;
                }
                finally
                {
                    _synclock.ExitReadLock();
                }
            }
        }

        private void RetrieveMetadata()
        {
            if (_syncAfter >= DateTimeOffset.UtcNow)
            {
                return;
            }

            _synclock.EnterWriteLock();
            try
            {
                var result = AsyncHelper.RunSync<OpenIdConnectConfiguration>(async () => await _configurationManager.GetConfigurationAsync());
                var tokens = from key in result.JsonWebKeySet.Keys
                            select new X509SecurityToken(new X509Certificate2(Convert.FromBase64String(key.X5c.First())));
                
                _issuer = result.Issuer;
                _tokens = tokens;
                _syncAfter = DateTimeOffset.UtcNow + _refreshInterval;
            }
            finally
            {
                _synclock.ExitWriteLock();
            }
        }
    }
}