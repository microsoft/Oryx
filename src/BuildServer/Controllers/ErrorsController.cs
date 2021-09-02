using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
            [Route("error")]
            public ErrorResponse Error()
            {
                var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
                var exception = context.Error; // Your exception
                var code = 500; // Internal Server Error by default

                //if (exception is OryxNotFoundException) code = 404; // Not Found
                //else if (exception is OryxUnauthException) code = 401; // Unauthorized
                //else if (exception is OryxException) code = 400; // Bad Request

                Response.StatusCode = code; // You can use HttpStatusCode enum instead

                return new ErrorResponse(exception); // error model
            }
    }
}
