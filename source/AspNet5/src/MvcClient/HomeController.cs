using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace MvcClient
{
    [Authorize]
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task Signout()
        {
            await HttpContext.Authentication.SignOutAsync("Oidc");
            await HttpContext.Authentication.SignOutAsync("Cookies");
        }

        public async Task<IActionResult> CallApi()
        {
            var token = User.FindFirst("access_token").Value;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetStringAsync("http://localhost:19806/identity");

            ViewBag.Json = JArray.Parse(response).ToString();
            return View();
        }
    }
}