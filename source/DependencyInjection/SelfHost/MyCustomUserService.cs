using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Thinktecture.IdentityServer.Core.Services;

namespace SelfHost
{
    public interface ICustomLogger
    {
        void Log(string message);
    }

    public class MyCustomDebugLogger : ICustomLogger
    {
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }

    public class MyCustomTokenSigningService: ITokenSigningService
    {
        Thinktecture.IdentityServer.Core.Configuration.IdentityServerOptions options;
        ICustomLogger logger;

        public MyCustomTokenSigningService(
            Thinktecture.IdentityServer.Core.Configuration.IdentityServerOptions options,
            ICustomLogger logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public System.Threading.Tasks.Task<string> SignTokenAsync(Thinktecture.IdentityServer.Core.Connect.Models.Token token)
        {
            throw new NotImplementedException();
        }
    }
}
