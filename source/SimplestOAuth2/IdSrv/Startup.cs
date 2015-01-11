using Owin;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace IdSrv
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var factory = InMemoryFactory.Create(
                scopes:  Scopes.Get(),
                clients: Clients.Get(),
                users:   new List<InMemoryUser>());

            var options = new IdentityServerOptions
            {
                Factory = factory
            };

            app.UseIdentityServer(options);
        }
    }
}
