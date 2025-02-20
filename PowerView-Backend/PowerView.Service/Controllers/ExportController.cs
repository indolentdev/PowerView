using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ISeriesNameRepository seriesNameRepository;
    private readonly IExportRepository exportRepository;
    private readonly ILocationContext locationContext;

    public ExportController(ILogger<ExportController> logger, ISeriesNameRepository seriesNameRepository, IExportRepository exportRepository, ILocationContext locationContext)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.seriesNameRepository = seriesNameRepository ?? throw new ArgumentNullException(nameof(seriesNameRepository));
        this.exportRepository = exportRepository ?? throw new ArgumentNullException(nameof(exportRepository));
        this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
    }

    [HttpGet("labels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetLabels()
    {
        var seriesNames = seriesNameRepository.GetSeriesNames();

        var r = seriesNames
          .Where(x => x.ObisCode.IsCumulative)
          .Select(x => x.Label)
          .Distinct()
          .ToList();
        return Ok(r);
    }

    [HttpGet("diffs/hourly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetHourlyDiffsExport(
        [BindRequired, FromQuery, UtcDateTime] DateTime from, 
        [BindRequired, FromQuery, UtcDateTime] DateTime to, 
        [BindRequired, FromQuery, MinLength(1)] string[] label)
    {
        if (from >= to) return BadRequest("to must be greater than from");
        var labelSeriesSet = exportRepository.GetLiveCumulativeSeries(from, to, label);
        var intervalGroup = new IntervalGroup(locationContext, from, "60-minutes", labelSeriesSet, Array.Empty<CostBreakdownGeneratorSeries>());
        intervalGroup.Prepare();

        var falttened = intervalGroup.NormalizedDurationLabelSeriesSet
          .OrderBy(x => x.Label)
          .SelectMany(x => x, (ls, oc) => new { ls.Label, ObisCode = oc, Values = ls[oc] })
          .Where(x => x.ObisCode.IsDelta)
          .SelectMany(x => x.Values, (x, value) => new { x.Label, x.ObisCode, NormalizedValue = value })
          .Where(x => x.NormalizedValue.NormalizedStart != x.NormalizedValue.NormalizedEnd);

        var periods = falttened
          .Select(x => new ExportPeriod(x.NormalizedValue.NormalizedStart, x.NormalizedValue.NormalizedEnd))
          .Distinct()
          .OrderBy(x => x)
          .ToList();

        var seriesGroups = falttened.GroupBy(x => new SeriesName(x.Label, x.ObisCode), x => x.NormalizedValue);
        var exportSeries = GetExportDiffSeries(periods, seriesGroups);

        var r = new { Periods = periods.Select(p => new { From = DateTimeMapper.Map(p.From), To = DateTimeMapper.Map(p.To) }).ToList(), 
          Series = exportSeries };
        return Ok(r);
    }

    private static IList<object> GetExportDiffSeries(IList<ExportPeriod> periods, IEnumerable<IGrouping<SeriesName, NormalizedDurationRegisterValue>> seriesGroups)
    {
        var exportSeries = new List<object>(); // matches dto

        foreach (var group in seriesGroups)
        {
            var seriesName = group.Key;

            var hourlyValues = periods
              .GroupJoin(group, x => x, x => new ExportPeriod(x.NormalizedStart, x.NormalizedEnd), (period, normalizedValues) => new { period, normalizedValues })
              .SelectMany(x => x.normalizedValues.DefaultIfEmpty(), (joinItem, normalizedValue) => new { joinItem.period, normalizedValue })
              .OrderBy(x => x.period)
              .Select(x => x.normalizedValue)
              .ToList();

            var completeHourlyValues = new
            {
                seriesName.Label,
                ObisCode = seriesName.ObisCode.ToString(),
                Values = hourlyValues.Select((x, i) =>
                {
                    if (x == null)
                    {
                        return new
                        {
                            From = DateTimeMapper.Map(null),
                            To = DateTimeMapper.Map(null),
                            Value = (double?)null,
                            Unit = (string)null
                        };
                    }

                    var unit = x.UnitValue.Unit;
                    return new
                    {
                        From = DateTimeMapper.Map(x.Start),
                        To = DateTimeMapper.Map(x.End),
                        Value = ValueAndUnitConverter.Convert(x.UnitValue.Value, unit),
                        Unit = ValueAndUnitConverter.Convert(unit)
                    };
                }).ToList()
            };
            exportSeries.Add(completeHourlyValues);
        }

        return exportSeries;
    }

    [HttpGet("gauges/hourly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetHourlyGaugesExport(
        [BindRequired, FromQuery, UtcDateTime] DateTime from,
        [BindRequired, FromQuery, UtcDateTime] DateTime to,
        [BindRequired, FromQuery, MinLength(1)] string[] label)
    {
        if (from >= to) return BadRequest("to must be greater than from");
        var labelSeriesSet = exportRepository.GetLiveCumulativeSeries(from, to, label);
        var intervalGroup = new IntervalGroup(locationContext, from, "60-minutes", labelSeriesSet, Array.Empty<CostBreakdownGeneratorSeries>());
        intervalGroup.Prepare();

        var falttened = intervalGroup.NormalizedLabelSeriesSet
          .OrderBy(x => x.Label)
          .SelectMany(x => x, (ls, oc) => new { ls.Label, ObisCode = oc, Values = ls[oc] })
          .SelectMany(x => x.Values, (x, value) => new { x.Label, x.ObisCode, NormalizedValue = value });

        var hours = falttened
          .Select(x => x.NormalizedValue.NormalizedTimestamp)
          .Distinct()
          .OrderBy(x => x)
          .ToList();

        var seriesGroups = falttened.GroupBy(x => new SeriesName(x.Label, x.ObisCode), x => x.NormalizedValue);
        var exportSeries = GetExportGaugeSeries(hours, seriesGroups);

        var r = new { Timestamps = hours.Select(DateTimeMapper.Map).ToList(), Series = exportSeries };
        return Ok(r);
    }

    private static IList<object> GetExportGaugeSeries(IList<DateTime> hours, IEnumerable<IGrouping<SeriesName, NormalizedTimeRegisterValue>> seriesGroups)
    {
        var exportSeries = new List<object>(); // matches dto

        foreach (var group in seriesGroups)
        {
            var seriesName = group.Key;

            var hourlyValues = hours
              .GroupJoin(group, x => x, x => x.NormalizedTimestamp, (hour, normalizedValues) => new { hour, normalizedValues })
              .SelectMany(x => x.normalizedValues.DefaultIfEmpty(), (joinItem, normalizedValue) => new { joinItem.hour, normalizedValue })
              .OrderBy(x => x.hour)
              .Select(x => x.normalizedValue)
              .ToList();

            var completeHourlyValues = new
            {
                seriesName.Label,
                ObisCode = seriesName.ObisCode.ToString(),
                Values = hourlyValues.Select((x, i) =>
                {
                    if (x == null)
                    {
                        return new
                        {
                            Timestamp = DateTimeMapper.Map(null),
                            Value = (double?)null,
                            Unit = (string)null,
                            DeviceId = (string)null
                        };
                    }

                    var unit = x.TimeRegisterValue.UnitValue.Unit;
                    return new
                    {
                        Timestamp = DateTimeMapper.Map(x.TimeRegisterValue.Timestamp),
                        Value = ValueAndUnitConverter.Convert(x.TimeRegisterValue.UnitValue.Value, unit),
                        Unit = ValueAndUnitConverter.Convert(unit),
                        DeviceId = x.TimeRegisterValue.DeviceId
                    };
                }).ToList()
            };
            exportSeries.Add(completeHourlyValues);
        }

        return exportSeries;
    }

    private DateTime? GetDateTime(string parameterValue, string parameterName)
    {
        DateTime timestampParse;
        if (!DateTime.TryParse(parameterValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out timestampParse) || timestampParse.Kind != DateTimeKind.Utc)
        {
            logger.LogInformation($"Unable to parse query parameter {parameterName} as UTC timestamp date time string:{parameterValue}");
            return null;
        }
        else
        {
            return timestampParse;
        }
    }

}
