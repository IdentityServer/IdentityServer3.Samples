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
            // this verifies that we have a prtial signin from idsvr
            var ctx = Request.GetOwinContext();
            var authentication = await ctx.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (authentication == null)
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
            var authentication = await ctx.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (authentication == null)
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

            // add a cookie with the hrd value
            Request.GetOwinContext().Response.Cookies.Append("idp", idp);
            // find the custom URL to return to the login page
            var resumeUrl = authentication.Identity.Claims.Single(x => x.Type == "url").Value;
            // clear the partial cookie because we're working around a limitation in partial redirects for hrd
            Request.GetOwinContext().Authentication.SignOut(Constants.PartialSignInAuthenticationType);
            // return the the login page
            return Redirect(resumeUrl);
        }
    }
}
