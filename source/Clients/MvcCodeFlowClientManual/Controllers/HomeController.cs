using Sample;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Client;

namespace MvcCodeFlowClientManual.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Request.GetOwinContext().Authentication.SignOut("Cookies");

            return View();
        }

        [HttpPost]
        public ActionResult Index(string scopes)
        {
            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");

            var client = new OAuth2Client(new Uri(Constants.AuthorizeEndpoint));
            
            var url = client.CreateCodeFlowUrl(
                clientId:    "codeclient",
                scope:        scopes,
                redirectUri: "https://localhost:44312/callback",
                state:       state,
                nonce:       nonce);

            return Redirect(url);
        }
    }
}