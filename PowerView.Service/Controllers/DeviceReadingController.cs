using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model.Repository;
using PowerView.Model;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/devices")]
public class DeviceReadingController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IReadingAccepter readingAccepter;

    public DeviceReadingController(ILogger<DeviceReadingController> logger, IReadingAccepter readingAccepter)
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
                new Reading(x.Label, x.DeviceId != null ? x.DeviceId : x.SerialNumber.ToString(), x.Timestamp.Value,
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

    [HttpPost("manualregisters")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PostManualRegisterValue(
        [BindRequired, FromBody] PostCrudeValueDto crudeValue
    )
    {
        var reading = new Reading(crudeValue.Label, crudeValue.DeviceId, crudeValue.Timestamp.Value,
          new[] { new RegisterValue(crudeValue.ObisCode, crudeValue.Value.Value, crudeValue.Scale.Value, UnitMapper.Map(crudeValue.Unit), RegisterValueTag.Manual) });

        try
        {
            readingAccepter.Accept(new [] { reading });
        }
        catch (DataStoreUniqueConstraintException e)
        {
            var statusCode = StatusCodes.Status409Conflict;
            var msg = $"Unable to add manual register for label:{reading.Label}. Duplicate entry. Responding {statusCode}";
            Exception ex = null;
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ex = e;
            }
            logger.LogInformation(ex, msg);
            return StatusCode(statusCode, "Conflict");
        }

        return NoContent();
    }

}
