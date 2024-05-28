using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreApp22WebApp.Controllers
{
    [Route("/")]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
                var responseContent = "Hello World!";

// For verifying scenarios where we want to make sure this app is being using Debug configuration.
#if DEBUG
                responseContent += " from Debug build.";
#endif

                return responseContent;
        }
    }
}