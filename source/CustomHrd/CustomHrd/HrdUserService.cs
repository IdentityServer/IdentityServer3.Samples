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

        public override async Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            var user = await owinContext.Environment.GetIdentityServerPartialLoginAsync();
            if (user == null)
            {
                // no idp, so do partial login to HRD page
                context.AuthenticateResult = new AuthenticateResult("~/hrd", (IEnumerable<Claim>)null);
            }
            else
            {
                // we have partial login, so look for IdP claim
                var idp_claim = user.Claims.FirstOrDefault(x => x.Type == "idp");
                if (idp_claim == null)
                {
                    context.AuthenticateResult = new AuthenticateResult("Error: no IdP claim found");
                }

                context.SignInMessage.IdP = idp_claim.Value;
            }
        }
    }
}
