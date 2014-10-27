/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.MembershipReboot;

namespace SelfHost
{
    public class MembershipRebootUserServiceFactory
    {
        public static IUserService Factory(string connString)
        {
            var db = new DefaultMembershipRebootDatabase(connString);
            var repo = new DefaultUserAccountRepository(db);
            var userAccountService = new UserAccountService(config, repo);
            var userSvc = new MembershipRebootUserService<UserAccount>(userAccountService, db);
            return userSvc;
        }

        static MembershipRebootConfiguration config;
        static MembershipRebootUserServiceFactory()
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());

            config = new MembershipRebootConfiguration();
            config.PasswordHashingIterationCount = 50000;
            config.AllowLoginAfterAccountCreation = true;
            config.RequireAccountVerification = false;
        }
    }
}
