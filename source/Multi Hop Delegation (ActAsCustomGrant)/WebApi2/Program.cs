using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:44335"))
            {
                Console.WriteLine("server running...");
                Console.ReadLine();
            }
        }
    }
}
