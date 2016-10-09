using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using SampleApp.Repositories;

namespace SampleApp.Services
{
    public class U2FLoginUserService : UserServiceBase
    {
        private readonly IUserRepository _userRepository;

        public U2FLoginUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var user = _userRepository.FindUser(context.UserName);
            if (user == null)
            {
                return Task.FromResult(0);
            }

            context.AuthenticateResult = !user.Devices.Any()
                ? new AuthenticateResult("~/u2fregister", user.Subject, user.Username)
                : new AuthenticateResult("~/u2fauthenticate", user.Subject, user.Username);

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // issue the claims for the user
            var user = _userRepository.FindUserById(context.Subject.GetSubjectId());
            if (user != null)
            {
                context.IssuedClaims = user.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            }

            return Task.FromResult(0);
        }
    }
}
