using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PowerView.Model;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/application")]
public class SettingsApplicationController : ControllerBase
{
    private readonly ILocationContext locationContext;

    public SettingsApplicationController(ILocationContext locationContext)
    {
        this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetProps()
    {
        var version = GetVersion();
        var cultureInfo = locationContext.CultureInfo;
        var timeZoneDisplayName = locationContext.GetTimeZoneDisplayName();

        var r = new { Version = version, Culture = cultureInfo.NativeName, TimeZone = timeZoneDisplayName };
        return Ok(r);
    }

    private string GetVersion()
    {
        return GetType().Assembly.GetName().Version.ToString(3);
    }

}
