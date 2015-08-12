using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.EntityFramework;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;

namespace SelfHost.Config
{
    class Factory
    {
        public static IdentityServerServiceFactory Configure(string connString)
        {
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = connString,
            };

            // these two calls just pre-populate the test DB from the in-memory config
            ConfigureClients(Clients.Get(), efConfig);
            ConfigureScopes(Scopes.Get(), efConfig);

            var factory = new IdentityServerServiceFactory();

            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);

            factory.UseInMemoryUsers(Users.Get());

            return factory;
        }

        public static void ConfigureClients(IEnumerable<Client> clients, EntityFrameworkServiceOptions options)
        {
            using (var db = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Clients.Any())
                {
                    foreach (var c in clients)
                    {
                        var e = c.ToEntity();
                        db.Clients.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void ConfigureScopes(IEnumerable<Scope> scopes, EntityFrameworkServiceOptions options)
        {
            using (var db = new ScopeConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Scopes.Any())
                {
                    foreach (var s in scopes)
                    {
                        var e = s.ToEntity();
                        db.Scopes.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}
