﻿using System.ComponentModel.DataAnnotations;
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
    private readonly IReadingHistoryRepository readingHistoryRepository;

    public DeviceReadingController(ILogger<DeviceReadingController> logger, IReadingAccepter readingAccepter, IReadingHistoryRepository readingHistoryRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readingAccepter = readingAccepter ?? throw new ArgumentNullException(nameof(readingAccepter));
        this.readingHistoryRepository = readingHistoryRepository ?? throw new ArgumentNullException(nameof(readingHistoryRepository));
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

        logger.LogTrace($"Received {liveReadings.Sum(x => x.GetRegisterValues().Count)} values through device readings api");

        var runAccept = true;
        var dataStoreBusyExceptionCount = 0;
        while (runAccept && dataStoreBusyExceptionCount <= 1)
        {
            try
            {
                readingAccepter.Accept(liveReadings);
                runAccept = false;
            }
            catch (DataStoreBusyException e)
            {
                dataStoreBusyExceptionCount++;
                if (dataStoreBusyExceptionCount == 1)
                {
                    logger.LogTrace(e, "First chance exception accepting readings. Retrying once.");
                    Task.Delay(350).GetAwaiter().GetResult();
                }
                else
                {
                    var statusCode = StatusCodes.Status503ServiceUnavailable;
                    var msg = $"Unable to add readings for label(s):{string.Join(",", liveReadings.Select(r => r.Label))}. Data store busy. Responding {statusCode}. {e.InnerException?.Message}";
                    Exception ex = null;
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        ex = e;
                    }
                    logger.LogInformation(ex, msg);
                    return StatusCode(statusCode, "Data store busy");
                }
            }
        }

        return NoContent();
    }

    [HttpPost("manualregisters")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PostManualRegisterValue(
        [BindRequired, FromBody] LabelRegisterValueDto labelRegisterValue
    )
    {
        var reading = new Reading(labelRegisterValue.Label, labelRegisterValue.DeviceId, labelRegisterValue.Timestamp.Value,
          new[] { new RegisterValue(labelRegisterValue.ObisCode, labelRegisterValue.Value.Value, labelRegisterValue.Scale.Value, UnitMapper.Map(labelRegisterValue.Unit), RegisterValueTag.Manual) });

        logger.LogTrace($"Received {reading.GetRegisterValues().Count} values through manual readings api");

        try
        {
            readingAccepter.Accept(new[] { reading });
            readingHistoryRepository.ClearDayMonthYearHistory();
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
