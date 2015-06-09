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

        IOwinContext owinContext;
        public HrdUserService(OwinEnvironmentService env)
            : base(users)
        {
            this.owinContext = new OwinContext(env.Environment);
        }

        public override Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            var idp = owinContext.Request.Cookies["idp"];
            if (String.IsNullOrWhiteSpace(idp))
            {
                // no idp, so do partial login to HRD page
                var url = new Claim("url", owinContext.Request.Uri.AbsoluteUri);
                context.AuthenticateResult = new AuthenticateResult("~/hrd", "hrd", "hrd", new Claim[] { url });
            }
            else
            {
                // we have idp, so set it
                owinContext.Response.Cookies.Append("idp", ".", new CookieOptions { Expires = DateTime.UtcNow.AddYears(-1) });
                context.SignInMessage.IdP = idp;
            }

            return Task.FromResult(0);
        }
    }
}
