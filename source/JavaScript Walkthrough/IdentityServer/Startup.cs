namespace IdentityServer
{
    using Config;
    using IdentityServer3.Core.Configuration;
    using Owin;
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIdentityServer(new IdentityServerOptions
            {
                SiteName = "Embedded IdentityServer",
                SigningCertificate = LoadCertificate(),

                Factory = new IdentityServerServiceFactory()
                    .UseInMemoryUsers(Users.Get())
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get())
            });
        }

        private static X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"bin\Config\idsrv3test.pfx"), "idsrv3test");
        }
    }
}