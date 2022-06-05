using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/application")]
public class SettingsApplicationController : ControllerBase
{
    private readonly ILocationContext locationContext;

    public SettingsApplicationController(ILocationContext locationContext)
    {
        this.locationContext = locationContext ?? throw new ArgumentNullException("locationContext");
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetProps()
    {
        var version = GetVersion();
        var cultureInfo = locationContext.CultureInfo;
        var timeZone = locationContext.TimeZoneInfo;

        var r = new { Version = version, Culture = cultureInfo.NativeName, TimeZone = timeZone.DisplayName };
        return Ok(r);
    }

    private string GetVersion()
    {
        return GetType().Assembly.GetName().Version.ToString(3);
    }

}
