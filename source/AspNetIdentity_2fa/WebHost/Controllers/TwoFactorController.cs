using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityServer.Core;
using WebHost.AspId;

namespace WebHost.Controllers
{
    [Route("core/2fa")]
    public class TwoFactorController : Controller
    {
        UserManager userMgr;

        public TwoFactorController()
        {
            userMgr = new UserManager(new UserStore(new Context("name=AspId")));
        }

        public async Task<ActionResult> Index()
        {
            var ctx = Request.GetOwinContext();
            var authentication = await ctx.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (authentication == null)
            {
                return View("Error");
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Index(string code)
        {
            var ctx = Request.GetOwinContext();
            var authentication = await ctx.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (authentication == null)
            {
                return View("Error");
            }

            var id = authentication.Identity.Claims.Single(x => x.Type == Constants.ClaimTypes.Subject).Value;
            if (!(await this.userMgr.VerifyTwoFactorTokenAsync(id, "sms", code)))
            {
                ViewData["message"] = "Incorrect code";
                return View("Index");
            }

            var resumeUrl = authentication.Identity.Claims.Single(x => x.Type == Constants.ClaimTypes.PartialLoginReturnUrl).Value;
            return Redirect(resumeUrl);
        }
    }
}