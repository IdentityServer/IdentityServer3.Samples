using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core;
using WebHost.AspId;
using IdentityServer3.Core.Extensions;
using System.Security.Claims;

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
            var user = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Index(string code)
        {
            var ctx = Request.GetOwinContext();

            var user = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (user == null)
            {
                return View("Error");
            }

            var id = user.FindFirst("sub").Value;
            if (!(await this.userMgr.VerifyTwoFactorTokenAsync(id, "sms", code)))
            {
                ViewData["message"] = "Incorrect code";
                return View("Index");
            }

            var claims = user.Claims.Where(c => c.Type != "amr").ToList();
            claims.Add(new Claim("amr", "2fa"));
            await ctx.Environment.UpdatePartialLoginClaimsAsync(claims);

            var resumeUrl = await ctx.Environment.GetPartialLoginResumeUrlAsync();
            return Redirect(resumeUrl);
        }
    }
}