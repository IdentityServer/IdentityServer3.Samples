using SampleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Extensions;

namespace SampleApp.Controllers
{
    public class RegisterController : Controller
    {
        [Route("core/register")]
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

        [Route("core/register")]
        [HttpPost]
        public async Task<ActionResult> Index(RegisterModel model)
        {
            var ctx = Request.GetOwinContext();
            var authentication = await ctx.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (authentication == null)
            {
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                // update the "database" for our users with the registration data
                var subject = authentication.Identity.GetSubjectId();
                var user = RegistrationUserService.Users.Single(x => x.Subject == subject);
                user.Claims.Add(new Claim(Constants.ClaimTypes.GivenName, model.First));
                user.Claims.Add(new Claim(Constants.ClaimTypes.FamilyName, model.Last));

                // replace the name captured from the external identity provider
                var nameClaim = user.Claims.Single(x => x.Type == Constants.ClaimTypes.Name);
                user.Claims.Remove(nameClaim);
                nameClaim = new Claim(Constants.ClaimTypes.Name, model.First + " " + model.Last);
                user.Claims.Add(nameClaim);

                // mark user as registered
                user.IsRegistered = true;
                
                // this replaces the name issued in the partial signin cookie
                // the reason for doing is so when we redriect back to IdSvr it will
                // use the updated name for display purposes. this is only needed if
                // the registration process needs to use a different name than the one
                // we captured from the external provider
                var partialClaims = authentication.Identity.Claims.Where(x => x.Type != Constants.ClaimTypes.Name).ToList();
                partialClaims.Add(nameClaim);
                ctx.Authentication.SignIn(new ClaimsIdentity(partialClaims, Constants.PartialSignInAuthenticationType));

                // find the URL to continue with the process to the issue the token to the RP
                var resumeUrl = authentication.Identity.Claims.Single(x => x.Type == Constants.ClaimTypes.PartialLoginReturnUrl).Value;
                return Redirect(resumeUrl);
            }

            return View();
        }
    }
}
