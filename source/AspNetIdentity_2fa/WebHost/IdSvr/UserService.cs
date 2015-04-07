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

using WebHost.AspId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.AspNetIdentity;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Models;

namespace WebHost.IdSvr
{
    public static class UserServiceExtensions
    {
        public static void ConfigureUserService(this IdentityServerServiceFactory factory, string connString)
        {
            factory.UserService = new Registration<IUserService, UserService>();
            factory.Register(new Registration<UserManager>());
            factory.Register(new Registration<UserStore>());
            factory.Register(new Registration<Context>(resolver => new Context(connString)));
        }
    }
    
    public class UserService : AspNetIdentityUserService<User, string>
    {
        public UserService(UserManager userMgr)
            : base(userMgr)
        {
        }

        protected override async Task<IdentityServer3.Core.Models.AuthenticateResult> PostAuthenticateLocalAsync(User user, SignInMessage message)
        {
            if (base.userManager.SupportsUserTwoFactor)
            {
                var id = user.Id;

                if (await userManager.GetTwoFactorEnabledAsync(id))
                {
                    var code = await this.userManager.GenerateTwoFactorTokenAsync(id, "sms");
                    var result = await userManager.NotifyTwoFactorTokenAsync(id, "sms", code);
                    if (!result.Succeeded)
                    {
                        return new IdentityServer3.Core.Models.AuthenticateResult(result.Errors.First());
                    }

                    var name = await GetDisplayNameForAccountAsync(id);
                    return new IdentityServer3.Core.Models.AuthenticateResult("~/2fa", id, name);
                }
            }

            return null;
        }
    }
}
