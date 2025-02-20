using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("")]
public class RootController : ControllerBase
{
    [HttpGet]
    [HttpGet("default.htm")]
    [HttpGet("default.html")]
    [HttpGet("index.htm")]
    [HttpGet("index.html")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public ActionResult Get()
    {
        return Redirect("web/index.html");
    }
}
