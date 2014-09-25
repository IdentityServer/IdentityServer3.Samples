using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace JsImplicitOAuthLibraryDemo.Api
{
    public class TestController : ApiController
    {
        [Route("api/test")]
        public IHttpActionResult Get()
        {
            var cp = (ClaimsPrincipal)User;
            var claims =
                from c in cp != null ? cp.Claims : Enumerable.Empty<Claim>()
                select new
                {
                    c.Type,
                    c.Value
                };
            return Ok(claims.ToArray());
        }
    }
}