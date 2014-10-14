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

//#define USE_INT_PRIMARYKEY

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Thinktecture.IdentityServer.AspNetIdentity;
using Thinktecture.IdentityServer.Core.Services;

namespace SelfHost
{
    public class AspNetIdentityUserServiceFactory
    {
        static AspNetIdentityUserServiceFactory()
        {
#if USE_INT_PRIMARYKEY
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<CustomDbContext>());
#else
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<IdentityDbContext>());
#endif
        }

        public static IUserService Factory(string connString)
        {
#if USE_INT_PRIMARYKEY
            connString += "_CustomPK";

            var db = new IdentityDbContext<CustomUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(connString);
            var store = new UserStore<CustomUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(db);
            var mgr = new UserManager<CustomUser, int>(store);
            mgr.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 3
            };
            var userSvc = new AspNetIdentityUserService<CustomUser, int>(mgr, db);
            return userSvc;
#else
            var db = new IdentityDbContext<IdentityUser>(connString);
            var store = new UserStore<IdentityUser>(db);
            var mgr = new UserManager<IdentityUser>(store);
            mgr.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 3
            };
            var userSvc = new AspNetIdentityUserService<IdentityUser, string>(mgr, db);
            return userSvc;
#endif
        }
    }
}
