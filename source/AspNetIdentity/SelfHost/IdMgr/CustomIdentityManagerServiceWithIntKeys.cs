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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SelfHost.AspId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Thinktecture.IdentityManager;
using Thinktecture.IdentityManager.AspNetIdentity;
using Thinktecture.IdentityManager.Configuration;

namespace SelfHost.IdMgr
{
    public static class CustomIdentityManagerServiceWithIntKeysExtensions
    {
        public static void ConfigureCustomIdentityManagerServiceWithIntKeys(this IdentityManagerServiceFactory factory, string connectionString)
        {
            factory.Register(new Registration<CustomContext>(resolver => new CustomContext(connectionString)));
            factory.Register(new Registration<CustomUserStore>());
            factory.Register(new Registration<CustomRoleStore>());
            factory.Register(new Registration<CustomUserManager>());
            factory.Register(new Registration<CustomRoleManager>());
            factory.IdentityManagerService = new Registration<IIdentityManagerService, CustomIdentityManagerServiceWithIntKeys>();
        }
    }

    public class CustomIdentityManagerServiceWithIntKeys : AspNetIdentityManagerService<CustomUser, int, CustomRole, int>
    {
        public CustomIdentityManagerServiceWithIntKeys(CustomUserManager userMgr, CustomRoleManager roleMgr)
            : base(userMgr, roleMgr)
        {
        }
    }
}