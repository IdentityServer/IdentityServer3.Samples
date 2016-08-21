namespace Api
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http;

    [Route("values")]
    public class ValuesController : ApiController
    {
        private static readonly Random _random = new Random();

        public IEnumerable<string> Get()
        {
            var random = new Random();

            return new[]
            {
                _random.Next(0, 10).ToString(),
                _random.Next(0, 10).ToString()
            };
        }
    }
}