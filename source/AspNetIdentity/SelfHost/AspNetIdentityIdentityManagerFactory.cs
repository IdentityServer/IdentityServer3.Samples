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
using SelfHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Thinktecture.IdentityManager;
using Thinktecture.IdentityServer.AspNetIdentity;

namespace Thinktecture.IdentityManager.Host
{
    public class AspNetIdentityIdentityManagerFactory
    {
        static AspNetIdentityIdentityManagerFactory()
        {
#if USE_INT_PRIMARYKEY
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<CustomDbContext>());
#else
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<IdentityDbContext>());
#endif

        }

        string connString;
        public AspNetIdentityIdentityManagerFactory(string connString)
        {
            this.connString = connString;
#if USE_INT_PRIMARYKEY
            this.connString += "_CustomPK";
#endif
        }

        public IIdentityManagerService Create()
        {
#if USE_INT_PRIMARYKEY
            var db = new IdentityDbContext<CustomUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(connString);
            var store = new UserStore<CustomUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(db);
            var usermgr = new UserManager<CustomUser, int>(store);
            usermgr.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 3
            };

            var rolestore = new RoleStore<CustomRole, int, CustomUserRole>(db);
            var rolemgr = new RoleManager<CustomRole, int>(rolestore);

            var svc = new Thinktecture.IdentityManager.AspNetIdentity.AspNetIdentityManagerService<CustomUser, int, CustomRole, int>(usermgr, rolemgr);
            var dispose = new DisposableIdentityManagerService(svc, db);
            return dispose;
#else
            var db = new IdentityDbContext<IdentityUser>(this.connString);
            var userstore = new UserStore<IdentityUser>(db);
            var usermgr = new Microsoft.AspNet.Identity.UserManager<IdentityUser>(userstore);
            usermgr.PasswordValidator = new Microsoft.AspNet.Identity.PasswordValidator
            {
                RequiredLength = 3
            };
            var rolestore = new RoleStore<IdentityRole>(db);
            var rolemgr = new Microsoft.AspNet.Identity.RoleManager<IdentityRole>(rolestore);

            var svc = new Thinktecture.IdentityManager.AspNetIdentity.AspNetIdentityManagerService<IdentityUser, string, IdentityRole, string>(usermgr, rolemgr);
            var dispose = new DisposableIdentityManagerService(svc, db);
            return dispose;
#endif
        }
    }
}