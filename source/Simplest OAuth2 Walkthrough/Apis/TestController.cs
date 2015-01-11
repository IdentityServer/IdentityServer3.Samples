using System.Web.Http;

namespace Apis
{
    [Route("test")]
    public class TestController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Json(new { message = "OK" });
        }
    }
}