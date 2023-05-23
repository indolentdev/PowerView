using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/data/crude")]
public class CrudeDataController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ICrudeDataRepository crudeDataRepository;
    private readonly IReadingHistoryRepository readingHistoryRepository;

    public CrudeDataController(ILogger<ExportController> logger, ICrudeDataRepository crudeDataRepository, IReadingHistoryRepository readingHistoryRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.crudeDataRepository = crudeDataRepository ?? throw new ArgumentNullException(nameof(crudeDataRepository));
        this.readingHistoryRepository = readingHistoryRepository ?? throw new ArgumentNullException(nameof(readingHistoryRepository));
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
            Values = crudeData.Result.Select(MapCrudeValue).ToList()
        };

        return Ok(r);
    }

    [HttpGet("by/{label}/{timestamp}/{obisCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetBy(
        [BindRequired, FromRoute, MinLength(1)] string label,
        [BindRequired, FromRoute, UtcDateTime] DateTime timestamp,
        [BindRequired, FromRoute, ObisCode] string obisCode
        )
    {
        var crudeData = crudeDataRepository.GetCrudeDataBy(label, timestamp, obisCode);

        if (crudeData == null)
        {
            return NoContent();
        }

        var r = MapCrudeValue(crudeData);

        return Ok(r);
    }

    private static object MapCrudeValue(CrudeDataValue crudeData)
    {
        return new
        {
            Timestamp = crudeData.DateTime,
            ObisCode = crudeData.ObisCode.ToString(),
            crudeData.Value,
            crudeData.Scale,
            Unit = UnitMapper.Map(crudeData.Unit),
            crudeData.DeviceId,
            Tags = GetFlags(crudeData.Tag).Select(x => x.ToString()).ToList()
        };
    }

    internal static IEnumerable<RegisterValueTag> GetFlags(RegisterValueTag input)
    {
        foreach (var value in Enum.GetValues<RegisterValueTag>()) 
        {
            if (input.HasFlag(value)) yield return value;
        }            
    }

    [HttpDelete("values/{label}/{timestamp}/{obisCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Delete(
        [BindRequired, FromRoute, MinLength(1)] string label,
        [BindRequired, FromRoute, UtcDateTime] DateTime timestamp,
        [BindRequired, FromRoute, ObisCode] string obisCode
        )
    {
        crudeDataRepository.DeleteCrudeData(label, timestamp, obisCode);
        readingHistoryRepository.ClearDayMonthYearHistory();

        return NoContent();
    }

    [HttpGet("missing-days")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetMissingDays(
        [BindRequired, FromQuery, MinLength(1)] string label,
        [BindRequired, FromQuery, ObisCode] string obisCode
        )
    {
        var missingDates = crudeDataRepository.GetMissingDays(label, obisCode);

        var r = missingDates
          .Select(x => new { x.Timestamp, x.PreviousTimestamp, x.NextTimestamp })
          .ToList();

        return Ok(r);
    }

}
