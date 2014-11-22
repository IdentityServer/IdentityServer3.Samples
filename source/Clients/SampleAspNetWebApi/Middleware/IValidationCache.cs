using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.v3.AccessTokenValidation
{
    public interface IValidationCache
    {
        Task AddAsync(string token, IEnumerable<Claim> claims);
        Task<IEnumerable<Claim>> GetAsync(string token);
    }
}