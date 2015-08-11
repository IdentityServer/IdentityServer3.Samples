using SampleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;

namespace SampleApp.Controllers
{
    public class HrdController : Controller
    {
        [Route("core/hrd")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // this verifies that we have a partial signin from idsvr
            var ctx = Request.GetOwinContext();
            var user = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (user == null)
            {
                return View("Error");
            }
            
            return View();
        }

        [Route("core/hrd")]
        [HttpPost]
        public async Task<ActionResult> Index(string email)
        {
            var ctx = Request.GetOwinContext();
            var user = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (user == null)
            {
                return View("Error");
            }

            var idp = "";
            if (email.Contains("gmail.com"))
            {
                idp = "Google";
            }
            if (email.Contains("twitter.com"))
            {
                idp = "Twitter";
            }
            if (email.Contains("facebook.com"))
            {
                idp = "Facebook";
            }

            if (String.IsNullOrWhiteSpace(idp))
            {
                ViewData["Error"] = "Unknown provider";
                return View();
            }

            // add hrd value as claim back to the partial login
            var claims = user.Claims.ToList();
            claims.Add(new Claim("idp", idp));
            await ctx.Environment.UpdatePartialLoginClaimsAsync(claims);

            // find the URL to return to the login page
            var resumeUrl = await ctx.Environment.GetPartialLoginRestartUrlAsync();
            
            // return the the login page
            return Redirect(resumeUrl);
        }
    }
}
