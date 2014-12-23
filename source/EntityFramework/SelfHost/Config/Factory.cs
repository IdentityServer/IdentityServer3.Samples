using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.EntityFramework;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Host.Config;

namespace SelfHost.Config
{
    class Factory
    {
        public static IdentityServerServiceFactory Configure(string connString)
        {
            var svcFactory = new EntityFrameworkServiceFactory(connString);
            svcFactory.ConfigureClients(Clients.Get());
            svcFactory.ConfigureScopes(Scopes.Get());

            var factory = new IdentityServerServiceFactory();
            factory.RegisterConfigurationServices(svcFactory);
            factory.RegisterOperationalServices(svcFactory);

            var userService = new Thinktecture.IdentityServer.Core.Services.InMemory.InMemoryUserService(Users.Get());
            factory.UserService = new Registration<IUserService>(resolver => userService);

            return factory;
        }
    }
}
