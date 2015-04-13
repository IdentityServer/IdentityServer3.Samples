using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace SampleWCFApiHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri address = new Uri("http://localhost:2728/Service1.svc");
            using (var host = new ServiceHost(typeof(Service1), address))
            {
                //// Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                host.Open();

                Console.WriteLine("The service is ready at {0}", address);
                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();
                host.Close();
            }
        }
    }
}
