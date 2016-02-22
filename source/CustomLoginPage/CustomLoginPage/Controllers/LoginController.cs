using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core.Extensions;
using System.Threading.Tasks;

namespace CustomLoginPage.Controllers
{
    public class LoginController : Controller
    {
        [Route("core/custom/login")]
        public ActionResult Index(string id)
        {
            return View();
        }

        [Route("core/custom/login")]
        [HttpPost]
        public ActionResult Index(string id, string sub, string name)
        {
            var env = Request.GetOwinContext().Environment;
            env.IssueLoginCookie(new IdentityServer3.Core.Models.AuthenticatedLogin
            {
                Subject = sub,
                Name = name,
            });

            var msg = env.GetSignInMessage(id);
            var returnUrl = msg.ReturnUrl;

            env.RemovePartialLoginCookie();

            return Redirect(returnUrl);
        }
    }
}