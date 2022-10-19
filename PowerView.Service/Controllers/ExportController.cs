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
        var seriesNames = seriesNameRepository.GetStoredSeriesNames();

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
        var labelSeriesSet = exportRepository.GetLiveCumulativeSeries(from, to, label);
        var intervalGroup = new IntervalGroup(locationContext.TimeZoneInfo, from, "60-minutes", labelSeriesSet);
        intervalGroup.Prepare();

        var falttened = intervalGroup.NormalizedDurationLabelSeriesSet
          .OrderBy(x => x.Label)
          .SelectMany(x => x, (ls, oc) => new { ls.Label, ObisCode = oc, Values = ls[oc] })
          .Where(x => x.ObisCode.IsDelta)
          .SelectMany(x => x.Values, (x, value) => new { x.Label, x.ObisCode, NormalizedValue = value })
          .Where(x => x.NormalizedValue.NormalizedStart != x.NormalizedValue.NormalizedEnd);

        var periods = falttened
          .Select(x => new Period(x.NormalizedValue.NormalizedStart, x.NormalizedValue.NormalizedEnd))
          .Distinct()
          .OrderBy(x => x)
          .ToList();

        var seriesGroups = falttened.GroupBy(x => new SeriesName(x.Label, x.ObisCode), x => x.NormalizedValue);
        var exportSeries = GetExportDiffSeries(periods, seriesGroups);

        var r = new { Periods = periods, Series = exportSeries };
        return Ok(r);
    }

    private static IList<object> GetExportDiffSeries(IList<Period> periods, IEnumerable<IGrouping<SeriesName, NormalizedDurationRegisterValue>> seriesGroups)
    {
        var exportSeries = new List<object>(); // matches dto

        foreach (var group in seriesGroups)
        {
            var seriesName = group.Key;

            var hourlyValues = periods
              .GroupJoin(group, x => x, x => new Period(x.NormalizedStart, x.NormalizedEnd), (period, normalizedValues) => new { period, normalizedValues })
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
                            From = (DateTime?)null,
                            To = (DateTime?)null,
                            Value = (double?)null,
                            Unit = (string)null
                        };
                    }

                    var unit = x.UnitValue.Unit;
                    return new
                    {
                        From = (DateTime?)x.Start,
                        To = (DateTime?)x.End,
                        Value = ValueAndUnitConverter.Convert(x.UnitValue.Value, unit),
                        Unit = ValueAndUnitConverter.Convert(unit)
                    };
                }).ToList()
            };
            exportSeries.Add(completeHourlyValues);
        }

        return exportSeries;
    }

    internal class Period : IEquatable<Period>, IComparable<Period>
    {
        public Period(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        public DateTime From { get; }
        public DateTime To { get; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Period [From:{0}, To:{1}]", From.ToString("o"), To.ToString("o"));
        }

        public int CompareTo(Period other)
        {
            if (From != other.From)
            {
                return From.CompareTo(other.From);
            }
            else
            {
                return To.CompareTo(other.To);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Period);
        }

        public bool Equals(Period other)
        {
            return other != null && From == other.From && To == other.To;
        }

        public override int GetHashCode()
        {
            var hashCode = -1781160927;
            hashCode = hashCode * -1521134295 + From.GetHashCode();
            hashCode = hashCode * -1521134295 + To.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Period period1, Period period2)
        {
            return EqualityComparer<Period>.Default.Equals(period1, period2);
        }

        public static bool operator !=(Period period1, Period period2)
        {
            return !(period1 == period2);
        }
    }

    [HttpGet("gauges/hourly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetHourlyGaugesExport(
        [BindRequired, FromQuery, UtcDateTime] DateTime from,
        [BindRequired, FromQuery, UtcDateTime] DateTime to,
        [BindRequired, FromQuery, MinLength(1)] string[] label)
    {
        var labelSeriesSet = exportRepository.GetLiveCumulativeSeries(from, to, label);
        var intervalGroup = new IntervalGroup(locationContext.TimeZoneInfo, from, "60-minutes", labelSeriesSet);
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

        var r = new { Timestamps = hours, Series = exportSeries };
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
                            Timestamp = (DateTime?)null,
                            Value = (double?)null,
                            Unit = (string)null,
                            DeviceId = (string)null
                        };
                    }

                    var unit = x.TimeRegisterValue.UnitValue.Unit;
                    return new
                    {
                        Timestamp = (DateTime?)x.TimeRegisterValue.Timestamp,
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
