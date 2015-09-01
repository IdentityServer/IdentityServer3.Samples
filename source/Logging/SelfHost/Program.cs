using Microsoft.Owin.Hosting;
using Serilog;
using System;

namespace SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "IdentityServer3 SelfHost";

            // log to both console and custom trace source sink
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole(outputTemplate: "{Timestamp:HH:MM} [{Level}] ({Name:l}){NewLine} {Message}{NewLine}{Exception}")
                .WriteTo.TraceSource(traceSourceName: "IdentityServer3")
                .CreateLogger();

            const string url = "https://localhost:44333/core";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("\n\nServer listening at {0}. Press enter to stop", url);
                Console.ReadLine();
            }
        }
    }
}