using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core.Extensions;
using Newtonsoft.Json;
using SampleApp.Extensions;
using SampleApp.Models.U2F;
using SampleApp.Services;

namespace SampleApp.Controllers
{
    public class U2FAuthenticateController : Controller
    {
        private readonly U2FService _u2FService;

        public U2FAuthenticateController()
        {
            _u2FService = new U2FService();
        }

        [Route("core/u2fauthenticate")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // this verifies that we have a partial signin from idsvr
            var ctx = Request.GetOwinContext();
            var partial_user = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (partial_user == null)
            {
                return View("Error");
            }

            try
            {
                // Create the U2F server challenge
                var serverChallenge = _u2FService.GenerateServerChallenges(partial_user.Name);

                if (serverChallenge == null || serverChallenge.Count == 0)
                {
                    throw new Exception("No server challenges were generated.");
                }

                var challenges = JsonConvert.SerializeObject(serverChallenge);

                var loginModel = new CompleteLoginModel
                {
                    AppId = serverChallenge.First().appId,
                    Version = serverChallenge.First().version,
                    UserName = partial_user.Name,
                    Challenges = challenges
                };

                return View(loginModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                ModelState.AddModelError("CustomError", e.Message);
                return View();
            }
        }

        [Route("core/u2fauthenticate/resume")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResumePartial(CompleteLoginModel model)
        {
            var ctx = Request.GetOwinContext();
            var partialUser = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (partialUser == null)
            {
                return View("Error");
            }

            try
            {
                if (!_u2FService.AuthenticateUser(model.UserName.Trim(), model.DeviceResponse.Trim()))
                {
                    throw new Exception("Device response did not work with user.");
                }

                // Update the amr claim to show that 2FA was used to sign in
                await ctx.Environment.UpdateAuthenticationMethodForU2FAsync();
                var url = await ctx.Environment.GetPartialLoginResumeUrlAsync();
                return Redirect(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                ModelState.AddModelError("", "Error authenticating");
                return View(model);
            }
        }
    }
}