using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public class InMemoryValidationCache : IValidationCache
    {
        private readonly IdentityServerBearerTokenAuthenticationOptions _options;
        private readonly MemoryCache _cache;

        public InMemoryValidationCache(IdentityServerBearerTokenAuthenticationOptions options)
        {
            _options = options;
            _cache = new MemoryCache("thinktecture.validationCache");
        }

        public Task AddAsync(string token, IEnumerable<Claim> claims)
        {
            _cache.Add(token, claims, DateTimeOffset.UtcNow.Add(_options.ValidationCacheDuration));

            return Task.FromResult<object>(null);
        }

        public Task<IEnumerable<Claim>> GetAsync(string token)
        {
            var result = _cache.Get(token);

            if (result != null)
            {
                return Task.FromResult(result as IEnumerable<Claim>);
            }

            return Task.FromResult<IEnumerable<Claim>>(null);
        }
    }
}