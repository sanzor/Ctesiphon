using ASPT.Routes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPT.Server.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class TemplateController:Controller {
        public TemplateController() {

        }
        [Route(Routes.Routes.GETALL)]
        [HttpGet]
        public async Task<ActionResult<string>> Get() {
            await Task.Delay(100);
            return "asasa";
        }
    }
}
