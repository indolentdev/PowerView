using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Service.Mappers;
using PowerView.Service.Dtos;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/devices")]
public class DeviceLiveReadingController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IReadingAccepter readingAccepter;

    public DeviceLiveReadingController(ILogger<DeviceLiveReadingController> logger, IReadingAccepter readingAccepter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readingAccepter = readingAccepter ?? throw new ArgumentNullException(nameof(readingAccepter));
    }

    [HttpPost("livereadings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult PostLiveReadings([FromBody] LiveReadingSetDto liveReadingSetDto)
    {
        var liveReadings = liveReadingSetDto.Items
            .Select(x =>
                new LiveReading(x.Label, x.DeviceId != null ? x.DeviceId : x.SerialNumber.ToString(), x.Timestamp.Value,
                    x.RegisterValues.Select(y => new RegisterValue(y.ObisCode, y.Value.Value, y.Scale.Value, y.Unit.Value))))
            .ToList();

        try
        {
            readingAccepter.Accept(liveReadings);
        }
        catch (DataStoreBusyException e)
        {
            var statusCode = StatusCodes.Status503ServiceUnavailable;
            var msg = $"Unable to add readings for label(s):{string.Join(",", liveReadings.Select(r => r.Label))}. Data store busy. Responding {statusCode}";
            Exception ex = null;
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ex = e;
            }
            logger.LogInformation(ex, msg);
            return StatusCode(statusCode, "Data store busy");
        }

        return NoContent();
    }

}
