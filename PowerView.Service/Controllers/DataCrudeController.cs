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
[Route("api/data/crude")]
public class CrudeDataController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ICrudeDataRepository crudeDataRepository;

    public CrudeDataController(ILogger<ExportController> logger, ICrudeDataRepository crudeDataRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.crudeDataRepository = crudeDataRepository ?? throw new ArgumentNullException(nameof(crudeDataRepository));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Get(
        [BindRequired, FromQuery, MinLength(1)] string label,
        [BindRequired, FromQuery, UtcDateTime] DateTime from 
        )
    {
        var crudeData = crudeDataRepository.GetCrudeData(label, from);

        var r = new {
            Label = label,
            TotalCount = crudeData.TotalCount,
            Values = crudeData.Result.Select(x => new { 
                Timestamp = x.DateTime, ObisCode = x.ObisCode.ToString(), x.Value, x.Scale, Unit = UnitMapper.Map(x.Unit), x.DeviceId
             }).ToList()
        };

        return Ok(r);
    }

    [HttpGet("by/{label}/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetBy(
        [BindRequired, FromRoute, MinLength(1)] string label,
        [BindRequired, FromRoute, UtcDateTime] DateTime timestamp
        )
    {
        var crudeData = crudeDataRepository.GetCrudeDataBy(label, timestamp);

        var r = crudeData.Select(x => new
        {
            Timestamp = x.DateTime,
            ObisCode = x.ObisCode.ToString(),
            x.Value,
            x.Scale,
            Unit = UnitMapper.Map(x.Unit),
            x.DeviceId
        }).ToList();

        return Ok(r);
    }

    [HttpGet("missing-days")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetMissingDays(
        [BindRequired, FromQuery, MinLength(1)] string label
        )
    {
        var missingDates = crudeDataRepository.GetMissingDays(label);

        var r = missingDates
          .Select(x => new { x.Timestamp, x.PreviousTimestamp, x.NextTimestamp })
          .ToList();

        return Ok(r);
    }

}
