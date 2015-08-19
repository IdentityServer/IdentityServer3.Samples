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
    public class LocalRegistrationController : Controller
    {
        [Route("core/localregistration")]
        [HttpGet]
        public ActionResult Index(string signin)
        {
            return View();
        }

        [Route("core/localregistration")]
        [HttpPost]
        public ActionResult Index(string signin, LocalRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new LocalRegistrationUserService.CustomUser
                {
                    Username = model.Username, 
                    Password = model.Password, 
                    Subject = Guid.NewGuid().ToString(),
                    Claims = new List<Claim>()
                };
                LocalRegistrationUserService.Users.Add(user);
                user.Claims.Add(new Claim(Constants.ClaimTypes.GivenName, model.First));
                user.Claims.Add(new Claim(Constants.ClaimTypes.FamilyName, model.Last));

                return Redirect("~/core/" + Constants.RoutePaths.Login + "?signin=" + signin);
            }

            return View();
        }
    }
}
