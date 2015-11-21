using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;

namespace Api.Controllers
{
    [Route("identity")]
    [Authorize]
    public class IdentityController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Json(from c in User.Claims
                        select new { c.Type, c.Value });
        }
    }
}