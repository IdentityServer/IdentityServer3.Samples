using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SampleApp.Config
{
    static class Certificate
    {
        internal static X509Certificate2 Get(string contentRootPath)
        {
            return new X509Certificate2(Path.Combine(contentRootPath, "Config\\idsrv3test.pfx"), "idsrv3test");
        }
    }
}