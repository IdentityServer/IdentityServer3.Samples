using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi2
{
    [Route("test")]
    public class TestController : ApiController
    {
        public IHttpActionResult Get()
        {
            var caller = User as ClaimsPrincipal;

            return Json(new
            {
                from = "WebApi2",
                message = "OK user",
                client = caller.FindFirst("client_id").Value,
                subject = caller.FindFirst("sub").Value,
                name = caller.Identity.Name
            });
        }
    }
}
