﻿/*
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
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using WebHost.IdSvr;
using Thinktecture.IdentityManager.Configuration;
using Thinktecture.IdentityServer.Core.Configuration;
using WebHost.IdMgr;
using Owin;

namespace WebHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //administrate users
            app.Map("/admin", adminApp =>
            {
                var factory = new IdentityManagerServiceFactory();
                factory.ConfigureSimpleIdentityManagerService("AspId");

                adminApp.UseIdentityManager(new IdentityManagerOptions()
                {
                    Factory = factory
                });
            });

            //authentication
            app.Map("/core", core =>
            {
                var idSvrFactory = Factory.Configure();
                idSvrFactory.ConfigureUserService("AspId");

                var options = new IdentityServerOptions
                {
                    SiteName = "Thinktecture IdentityServer3 - AspNetIdentity 2FA",
                    SigningCertificate = Certificate.Get(),
                    Factory = idSvrFactory,
                };

                core.UseIdentityServer(options);
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44333/core",

                ClientId = "mvc", 
                Scope = "openid profile read write",
                ResponseType = "id_token token",
                RedirectUri = "https://localhost:44333/",

                SignInAsAuthenticationType = "Cookies",
                UseTokenLifetime = false,
                
            });
        }
    }
}