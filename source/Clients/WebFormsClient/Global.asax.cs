using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace WebApp
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            // now you can fix up you session object from
            // if you use session state (which makes me sad if you do)
            var cp = (ClaimsPrincipal)HttpContext.Current.User;

        }
    }
}