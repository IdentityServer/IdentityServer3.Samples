using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;

namespace SampleApp
{
    public class HrdUserService : InMemoryUserService
    {
        static List<InMemoryUser> users = new List<InMemoryUser>();

        IOwinContext context;
        public HrdUserService(OwinEnvironmentService env)
            : base(users)
        {
            this.context = new OwinContext(env.Environment);
        }

        public override Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            var idp = context.Request.Cookies["idp"];
            if (String.IsNullOrWhiteSpace(idp))
            {
                // no idp, so redirect
                var url = new Claim("url", context.Request.Uri.AbsoluteUri);
                var result = new AuthenticateResult("~/hrd", "hrd", "hrd", new Claim[] { url });
                return Task.FromResult(result);
            }
            else
            {
                // we have idp, so set it
                context.Response.Cookies.Append("idp", ".", new CookieOptions { Expires = DateTime.UtcNow.AddYears(-1) });
                message.IdP = idp;
                return Task.FromResult<AuthenticateResult>(null);
            }
        }
    }
}
