using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;
using Nancy;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Modules
{
  public class ExportModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ISeriesNameRepository seriesNameRepository;
    private readonly IExportRepository exportRepository;
    private readonly ILocationContext locationContext;

    public ExportModule(ISeriesNameRepository seriesNameRepository, IExportRepository exportRepository, ILocationContext locationContext)
      : base("/api")
    {
      if (seriesNameRepository == null) throw new ArgumentNullException("seriesNameRepository");
      if (exportRepository == null) throw new ArgumentNullException("exportRepository");
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.seriesNameRepository = seriesNameRepository;
      this.exportRepository = exportRepository;
      this.locationContext = locationContext;

      Get["export/labels"] = GetLabels;
      Get["export/diffs/hourly"] = GetHourlyDiffsExport;
      Get["export/gauges/hourly"] = GetHourlyGaugesExport;
      Get["export/hourly"] = GetHourlyExport;
    }

    private dynamic GetLabels(dynamic param)
    {
      var seriesNames = seriesNameRepository.GetStoredSeriesNames();

      var r = seriesNames
        .Where(x => x.ObisCode.IsCumulative)
        .Select(x => x.Label)
        .Distinct()
        .ToList();
 
      return Response.AsJson(r);
    }

    private dynamic GetHourlyDiffsExport(dynamic param)
    {
      var fromDate = GetDateTime("from");
      var toDate = GetDateTime("to");
      var labels = GetStrings("labels");

      if (fromDate == null || toDate == null || labels == null || labels.Count == 0)
      {
        return HttpStatusCode.BadRequest;
      }

      var labelSeriesSet = exportRepository.GetLiveCumulativeSeries(fromDate.Value, toDate.Value, labels);
      var intervalGroup = new IntervalGroup(locationContext.TimeZoneInfo, fromDate.Value, "60-minutes", labelSeriesSet);
      intervalGroup.Prepare();

      var falttened = intervalGroup.NormalizedDurationLabelSeriesSet
        .OrderBy(x => x.Label)
        .SelectMany(x => x, (ls, oc) => new { ls.Label, ObisCode = oc, Values = ls[oc] })
        .Where(x => x.ObisCode.IsDelta)
        .SelectMany(x => x.Values, (x, value) => new { x.Label, x.ObisCode, NormalizedValue = value });

      var periods = falttened
        .Select(x => new Period(x.NormalizedValue.NormalizedStart, x.NormalizedValue.NormalizedEnd))
        .Distinct()
        .OrderBy(x => x)
        .ToList();

      var seriesGroups = falttened.GroupBy(x => new SeriesName(x.Label, x.ObisCode), x => x.NormalizedValue);
      var exportSeries = GetExportDiffSeries(periods, seriesGroups);

      var r = new { Periods = periods, Series = exportSeries };
      return Response.AsJson(r);
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
              Value = ValueAndUnitMapper.Map(x.UnitValue.Value, unit),
              Unit = ValueAndUnitMapper.Map(unit)
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

    private dynamic GetHourlyGaugesExport(dynamic param)
    {
      var fromDate = GetDateTime("from");
      var toDate = GetDateTime("to");
      var labels = GetStrings("labels");

      if (fromDate == null || toDate == null || labels == null || labels.Count == 0)
      {
        return HttpStatusCode.BadRequest;
      }

      var labelSeriesSet = exportRepository.GetLiveCumulativeSeries(fromDate.Value, toDate.Value, labels);
      var intervalGroup = new IntervalGroup(locationContext.TimeZoneInfo, fromDate.Value, "60-minutes", labelSeriesSet);
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
      return Response.AsJson(r);
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
              Value = ValueAndUnitMapper.Map(x.TimeRegisterValue.UnitValue.Value, unit),
              Unit = ValueAndUnitMapper.Map(unit),
              DeviceId = x.TimeRegisterValue.DeviceId
            };
          }).ToList()
        };
        exportSeries.Add(completeHourlyValues);
      }

      return exportSeries;
    }

    private dynamic GetHourlyExport(dynamic param)
    {
      var fromDate = GetDateTime("from");
      var toDate = GetDateTime("to");
      var labels = GetStrings("labels");

      if (fromDate == null || toDate == null || labels == null || labels.Count == 0)
      {
        return HttpStatusCode.BadRequest;
      }

      var labelSeriesSet = exportRepository.GetLiveCumulativeSeries(fromDate.Value, toDate.Value, labels);

      var dateTimeHelper = new DateTimeHelper(locationContext.TimeZoneInfo, fromDate.Value);
      var normalizedLabelSeriesSet = labelSeriesSet.Normalize(dateTimeHelper.GetDivider("60-minutes"));

      var falttened = normalizedLabelSeriesSet
        .OrderBy(x => x.Label)
        .SelectMany(x => x, (ls, oc) => new { ls.Label, ObisCode = oc, Values = ls[oc]})
        .SelectMany(x => x.Values, (x, value) => new { x.Label, x.ObisCode, NormalizedValue = value });

      var hours = falttened
        .Select(x => x.NormalizedValue.NormalizedTimestamp)
        .Distinct()
        .OrderBy(x => x)
        .ToList();

      var seriesGroups = falttened.GroupBy(x => new SeriesName(x.Label, x.ObisCode), x => x.NormalizedValue);
      var exportSeries = GetExportSeries(hours, seriesGroups);

      var r = new { Timestamps = hours, Series = exportSeries };
      return Response.AsJson(r);
    }

    private static IList<object> GetExportSeries(IList<DateTime> hours, IEnumerable<IGrouping<SeriesName, NormalizedTimeRegisterValue>> seriesGroups)
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
                DiffValue = (double?)null,
                Unit = (string)null,
                DeviceId = (string)null
              };
            }

            var diffValue = i > 0 ? CalculateDiffValue(x, hourlyValues[i - 1]) : null;
            var unit = x.TimeRegisterValue.UnitValue.Unit;
            return new
            {
              Timestamp = (DateTime?)x.TimeRegisterValue.Timestamp,
              Value = ValueAndUnitMapper.Map(x.TimeRegisterValue.UnitValue.Value, unit),
              DiffValue = diffValue != null ? ValueAndUnitMapper.Map(diffValue.Value, unit) : null,
              Unit = ValueAndUnitMapper.Map(unit),
              DeviceId = x.TimeRegisterValue.DeviceId
            };
          }).ToList()
        };
        exportSeries.Add(completeHourlyValues);
      }

      return exportSeries;
    }

    private static double? CalculateDiffValue(NormalizedTimeRegisterValue value, NormalizedTimeRegisterValue previousValue)
    {
      if (previousValue != null &&
        previousValue.TimeRegisterValue.DeviceIdEquals(value.TimeRegisterValue) &&
        previousValue.TimeRegisterValue.UnitValue.Unit == value.TimeRegisterValue.UnitValue.Unit)
      {
        var diffValueCandidate = value.TimeRegisterValue.UnitValue.Value - previousValue.TimeRegisterValue.UnitValue.Value;
        if (diffValueCandidate > 0)
        {
          return diffValueCandidate;
        }
      }
      return null;
    }

    private DateTime? GetDateTime(string queryParamName)
    {
      DateTime timestampParse;
      string timestampString = Request.Query[queryParamName];
      if (!DateTime.TryParse(timestampString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out timestampParse) ||
      timestampParse.Kind != DateTimeKind.Utc)
      {
        log.InfoFormat("Unable to parse query parameter {0} as UTC timestamp date time string:{1}", 
                         queryParamName, timestampString);
        return null;
      }
      else
      {
        return timestampParse;
      }
    }

    private IList<string> GetStrings(string queryParamName)
    {
      string queryParamValue = Request.Query[queryParamName];
      if (string.IsNullOrEmpty(queryParamValue))
      {
        return null;
      }var strings = queryParamValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
      return strings;
    }

  }
}
