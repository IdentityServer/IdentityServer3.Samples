using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using IdentityServer3.Core;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;

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
