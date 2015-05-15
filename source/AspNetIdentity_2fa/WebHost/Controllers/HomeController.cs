using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace WebHost.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Personal()
        {
            return View((User as ClaimsPrincipal).Claims);
        }

        [Route("home/logout")]
        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect("/");
        }
    }
}