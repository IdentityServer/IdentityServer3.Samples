using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core.Extensions;
using SampleApp.Models.U2F;
using SampleApp.Services;

namespace SampleApp.Controllers
{
    public class U2FRegistrationController : Controller
    {
        private readonly U2FService _u2FService;

        public U2FRegistrationController()
        {
            _u2FService = new U2FService();
        }

        [Route("core/u2fregister")]
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
                // First we need to generate a server challenge for the device to respond to
                var serverRegisterResponse = _u2FService.GenerateServerChallenge(partial_user.Name);
                var registerModel = new CompleteRegisterModel
                {
                    UserName = partial_user.Name,
                    AppId = serverRegisterResponse.AppId,
                    Challenge = serverRegisterResponse.Challenge,
                    Version = serverRegisterResponse.Version
                };

                return View("Register", registerModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ModelState.AddModelError("CustomError", e.Message);

                return View("Register");
            }
        }

        [Route("core/u2fregister")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteRegister(CompleteRegisterModel value)
        {
            var ctx = Request.GetOwinContext();
            var partial_user = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (partial_user == null)
            {
                return View("Error");
            }

            if (!string.IsNullOrWhiteSpace(value.DeviceResponse)
                && !string.IsNullOrWhiteSpace(value.UserName))
            {
                try
                {
                    value.DeviceResponse = _u2FService.CompleteRegistration(value.UserName.Trim(),
                        value.DeviceResponse.Trim())
                        ? "Registration was successful."
                        : "Registration failed.";

                    return View("CompletedRegister", new CompleteRegisterModel { UserName = value.UserName, DeviceResponse = value.DeviceResponse });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    ModelState.AddModelError("CustomError", e.Message);

                    return View("Register", value);
                }
            }

            ModelState.AddModelError("CustomError", "bad username/device response");
            return View("Register", value);
        }

        [Route("core/u2fregister/resume")]
        [HttpGet]
        public async Task<ActionResult> ResumePartial()
        {
            var ctx = Request.GetOwinContext();
            var partial_user = await ctx.Environment.GetIdentityServerPartialLoginAsync();
            if (partial_user == null)
            {
                return View("Error");
            }

            var url = await ctx.Environment.GetPartialLoginResumeUrlAsync();
            return Redirect(url);
        }
    }
}