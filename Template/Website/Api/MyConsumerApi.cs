namespace MY.MICROSERVICE.NAMEService
{
    using System;
    using Domain;
    using System.Linq;
    using System.ComponentModel;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Olive.Entities;
    using Olive.Mvc;
    using Microsoft.AspNetCore.Http;
    using Olive;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Controllers;

    /* NOTE ===================================
    *  See this before creating your API:
    *  https://github.com/Geeksltd/Olive/blob/master/Integration/Olive.Microservices/Integration.md
    *  ======================================== */

    [Route("api")]
    public class MyConsumerApi : BaseController
    {
        public class MyResultType
        {
            public string Property1;
            public string[] Property2;
        }

        [HttpGet, Route("my-function")]
        [Returns(typeof(MyResultType))]
        // [AuthorizeTrustedService]  or  [AuthorizeService("ServiceName")]  or  [Authorize(Roles = "...")]
        public async Task<IActionResult> MyFunction(string param1)
        {
            var result = new MyResultType
            {
                Property1 = "...",
                Property2 = new[] { "...", "..." },
            };

            return Json(result);
        }
    }
}