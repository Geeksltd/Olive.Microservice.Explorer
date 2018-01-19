namespace Controllers
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
    using Olive.Microservices;

    public class ApiController : BaseController
    {
        // Here you can add any app specific APIs.

        [HttpGet, Route("myUrl")]
        public async Task<IActionResult> MyApi(string param1)
        {
            var result = new
            {
                Property1 = "...",
                Property2 = new[] { "...", "..." },
            };

            return Json(result);
        }
    }
}
