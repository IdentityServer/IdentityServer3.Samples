/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
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
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<IdentityDbContext>());
            //System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<CustomDbContext>());
        }
        
        public static IUserService Factory(string connString)
        {
            var db = new IdentityDbContext<IdentityUser>(connString);
            var store = new UserStore<IdentityUser>(db);
            var mgr = new UserManager<IdentityUser>(store);
            var userSvc = new UserService<IdentityUser, string>(mgr, db);
            return userSvc;

            //var db = new CustomDbContext("CustomAspId");
            //var store = new CustomUserStore(db);
            //var mgr = new CustomUserManager(store);
            //var userSvc = new UserService<CustomUser, int>(mgr, db);
            //return userSvc;
        }
    }
}
