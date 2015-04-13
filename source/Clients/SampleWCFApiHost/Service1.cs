using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace SampleWCFApiHost
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public string GetIdentityData()
        {
            //var principal = User as ClaimsPrincipal;
            //return from c in principal.Identities.First().Claims
            //       select new
            //       {
            //           c.Type,
            //           c.Value
            //       };

            var claims = ((ClaimsIdentity)Thread.CurrentPrincipal.Identity).Claims;

            string result = "";
            foreach (Claim claim in claims)
            {
                result += "Claim Type: " + claim.Type.ToString();
                result += ", Claim Value: " + claim.Value.ToString();
                result += "\n\r";
            }

            return string.Format("Claims: {0}", result);
        }
    }
}
