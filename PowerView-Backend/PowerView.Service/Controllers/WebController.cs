using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace PowerView.Service.Controllers;

/// <summary>
/// Complementary route resolving for /web hierarchy 
/// Allows perma linking of web UI pages in the angular web application
/// </summary>
[ApiController]
[Route("web")]
public class WebController : ControllerBase
{
    private readonly IWebHostEnvironment env;

    public WebController(IWebHostEnvironment env)
    {
        this.env = env;
    }

    [HttpGet("{wildcard1}/{wildcard2}")]
    public ActionResult GetIndexHtml([FromRoute] string wildcard1, [FromRoute] string wildcard2)
    {
        var indexContentResult = GetIndexHtml();
        SetResponseNoCacheHeaders();
        return indexContentResult;
    }

    [HttpGet("{wildcard1}/{wildcard2}/{wildcard3}")]
    public ActionResult GetIndexHtml([FromRoute] string wildcard1, [FromRoute] string wildcard2, [FromRoute] string wildcard3)
    {
        var indexContentResult = GetIndexHtml();
        SetResponseNoCacheHeaders();
        return indexContentResult;
    }

    private ContentResult GetIndexHtml()
    {
        using var indexHtmlStream = env.WebRootFileProvider.GetFileInfo("index.html").CreateReadStream();
        using var indexHtmlStreamReader = new StreamReader(indexHtmlStream);
        var indexHtmlContent = indexHtmlStreamReader.ReadToEnd();
        return Content(indexHtmlContent, "text/html", indexHtmlStreamReader.CurrentEncoding);
    }

    private void SetResponseNoCacheHeaders()
    {
        Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        Response.Headers.Append("Pragma", "no-cache");
    }

}
