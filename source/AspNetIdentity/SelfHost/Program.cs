using Microsoft.Owin.Hosting;
using System;

namespace SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "IdentityServer3 SelfHost";

            const string url = "https://localhost:44333/core";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("\n\nServer listening at {0}. Press enter to stop", url);
                Console.ReadLine();
            }
        }
    }
}