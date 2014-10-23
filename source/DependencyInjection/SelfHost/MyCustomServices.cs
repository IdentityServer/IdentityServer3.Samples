using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

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

    public class MyCustomClaimsProvider : DefaultClaimsProvider
    {
        ICustomLogger logger;

        public MyCustomClaimsProvider(ICustomLogger logger, IUserService userSvc)
            : base(userSvc)
        {
            this.logger = logger;
            this.logger.Log("yay, custom type was injected");
        }
    }
}
