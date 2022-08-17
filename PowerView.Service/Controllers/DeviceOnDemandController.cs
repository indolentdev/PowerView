using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Service.DisconnectControl;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/devices")]
public class DeviceOnDemandController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IDisconnectControlCache disconnectControlCache;

    public DeviceOnDemandController(ILogger<DeviceOnDemandController> logger, IDisconnectControlCache disconnectControlCache)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.disconnectControlCache = disconnectControlCache ?? throw new ArgumentNullException(nameof(disconnectControlCache));
    }

    [HttpGet("ondemand")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetOnDemand([BindRequired, FromQuery, StringLength(50, MinimumLength = 1)] string label)
    {
        // DisconnectControl on demand
        var outputStatus = disconnectControlCache.GetOutputStatus(label);

        var items = outputStatus.Select(x => new
        {
            Label = x.Key.Label,
            ObisCode = x.Key.ObisCode.ToString(),
            Kind = "Method",
            Index = (x.Value ? 1 : 0) + 1 // Cosem DisconnectControl object. Method 1=remote_disconnect, Method 2=remote_reconnect
        }).ToList();
        return Ok(new { Items = items });
    }

}