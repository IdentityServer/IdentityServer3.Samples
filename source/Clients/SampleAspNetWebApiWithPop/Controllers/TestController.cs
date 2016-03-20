using System.Collections.Generic;
using System.Web.Http;

namespace SampleAspNetWebApiWithPop.Controllers
{
    public class TestController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}