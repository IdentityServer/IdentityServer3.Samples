using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace SampleApp
{
    public class UserService : InMemoryUserService
    {
        IOwinContext ctx;

        public UserService(List<InMemoryUser> users, OwinEnvironmentService env) : base(users)
        {
            ctx = new OwinContext(env.Environment);
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var form = await ctx.Request.ReadFormAsync();
            var extra = form["extra"];

            await base.AuthenticateLocalAsync(context);
        }
    }
}