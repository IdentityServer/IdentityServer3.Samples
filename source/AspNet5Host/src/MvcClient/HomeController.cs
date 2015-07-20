using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace MvcClient
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
