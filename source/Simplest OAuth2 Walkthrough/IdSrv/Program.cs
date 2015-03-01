using Microsoft.Owin.Hosting;
using System;
using Thinktecture.IdentityServer.Core.Logging;

namespace IdSrv
{
    class Program
    {
        static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());

            // The port here must be one between 44300 and 44399 (or just 443), or else no request will be processed.
            using (WebApp.Start<Startup>("https://localhost:44333"))
            {
                Console.WriteLine("server running...");
                Console.ReadLine();
            }
        }
    }
}
