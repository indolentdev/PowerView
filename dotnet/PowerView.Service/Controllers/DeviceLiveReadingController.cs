using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/devices")]
public class DeviceLiveReadingController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ILiveReadingMapper liveReadingMapper;
    private readonly IReadingAccepter readingAccepter;

    public DeviceLiveReadingController(ILogger<DeviceLiveReadingController> logger, ILiveReadingMapper liveReadingMapper, IReadingAccepter readingAccepter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.liveReadingMapper = liveReadingMapper ?? throw new ArgumentNullException(nameof(liveReadingMapper));
        this.readingAccepter = readingAccepter ?? throw new ArgumentNullException(nameof(readingAccepter));
    }

    [HttpPost("livereadings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult PostLiveReadings()
    {
        // TODO: Have ASP.NET Core deserialize the dto..
        var liveReadings = liveReadingMapper.Map(Request.Headers.ContentType, Request.Body).ToList();

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
