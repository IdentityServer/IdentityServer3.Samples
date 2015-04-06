using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;

namespace SampleApp
{
    public class UserService : InMemoryUserService
    {
        IOwinContext ctx;

        public UserService(List<InMemoryUser> users, OwinEnvironmentService env) : base(users)
        {
            ctx = new OwinContext(env.Environment);
        }

        public async override System.Threading.Tasks.Task<IdentityServer3.Core.Models.AuthenticateResult> AuthenticateLocalAsync(string username, string password, IdentityServer3.Core.Models.SignInMessage message)
        {
            var form = await ctx.Request.ReadFormAsync();
            var extra = form["extra"];

            return await base.AuthenticateLocalAsync(username, password, message);
        }
    }
}