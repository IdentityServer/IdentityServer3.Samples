using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using System.Security.Claims;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using IdentityServer3.Core;

namespace CustomLoginPage
{
    public class CustomLoginPageUserService : UserServiceBase
    {
        OwinContext ctx;
        public CustomLoginPageUserService(OwinEnvironmentService owinEnv)
        {
            ctx = new OwinContext(owinEnv.Environment);
        }

        public override Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            var id = ctx.Request.Query.Get("signin");
            context.AuthenticateResult = new AuthenticateResult("~/custom/login?id=" + id, (IEnumerable<Claim>)null);
            return Task.FromResult(0);
        }
    }
}
