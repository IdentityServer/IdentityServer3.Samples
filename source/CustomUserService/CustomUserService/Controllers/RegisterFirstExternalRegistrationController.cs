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
    public class RegisterFirstExternalRegistrationController : Controller
    {
        [Route("core/registerfirstexternalregistration")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // this verifies that we have a partial signin from idsvr
            var ctx = Request.GetOwinContext();
            var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (partial_login == null)
            {
                return View("Error");
            }
            
            return View();
        }

        [Route("core/registerfirstexternalregistration")]
        [HttpPost]
        public async Task<ActionResult> Index(ExternalRegistrationModel model)
        {
            var ctx = Request.GetOwinContext();
            var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (partial_login == null)
            {
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                // update the "database" for our users with the registration data
                var nameIdClaim = partial_login.Claims.First(x => x.Type == Constants.ClaimTypes.ExternalProviderUserId);
                var provider = nameIdClaim.Issuer;
                var providerUserId = nameIdClaim.Value;

                var user = new SampleApp.RegisterFirstExternalRegistrationUserService.CustomUser
                {
                    Subject = Guid.NewGuid().ToString(),
                    Provider = provider,
                    ProviderID = providerUserId,
                    Claims = new List<Claim> { 
                        new Claim(Constants.ClaimTypes.Name, model.First + " " + model.Last),
                        new Claim(Constants.ClaimTypes.GivenName, model.First),
                        new Claim(Constants.ClaimTypes.FamilyName, model.Last),
                    }
                };
                
                SampleApp.RegisterFirstExternalRegistrationUserService.Users.Add(user);

                // find the URL to continue with the process to the issue the token to the RP
                var resumeUrl = await ctx.Environment.GetPartialLoginResumeUrlAsync();
                return Redirect(resumeUrl);
            }

            return View();
        }
    }
}
