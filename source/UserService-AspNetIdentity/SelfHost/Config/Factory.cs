using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Host.Config;

namespace SelfHost.Config
{
    class Factory
    {
        public static IdentityServerServiceFactory Configure(string connString)
        {
            var factory = new IdentityServerServiceFactory();

            factory.UserService = Registration<IUserService>.RegisterFactory(()=>AspNetIdentityUserServiceFactory.Factory(connString));

            var scopeSvc = new InMemoryScopeService(Scopes.Get());
            factory.ScopeService = Registration.RegisterFactory<IScopeService>(() => scopeSvc);
            var clientSvc = new InMemoryClientService(Clients.Get());
            factory.ClientService = Registration.RegisterFactory<IClientService>(() => clientSvc);

            return factory;
        }
    }
}
