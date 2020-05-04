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

    private readonly ILabelRepository labelRepository;
    private readonly IExportRepository exportRepository;
    private readonly ILocationContext locationContext;

    public ExportModule(ILabelRepository labelRepository, IExportRepository exportRepository, ILocationContext locationContext)
      : base("/api")
    {
      if (labelRepository == null) throw new ArgumentNullException("labelRepository");
      if (exportRepository == null) throw new ArgumentNullException("exportRepository");
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.labelRepository = labelRepository;
      this.exportRepository = exportRepository;
      this.locationContext = locationContext;

      Get["export/labels"] = GetLabels;
      Get["export/hourly"] = GetHourlyExport;
    }

    private dynamic GetLabels(dynamic param)
    {
      var r = labelRepository.GetLabels();
 
      return Response.AsJson(r);
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
        .OrderBy(x => x);

      var exportSeries = new List<object>(); // matches dto

      var seriesGroups = falttened.GroupBy(x => new SeriesName(x.Label, x.ObisCode), x => x.NormalizedValue);
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
            var diffValue = i > 0 ? CalculateDiffValue(x, hourlyValues[i-1]) : null;
            var unit = x.TimeRegisterValue.UnitValue.Unit;
            return new 
            { 
              Timestamp = x.TimeRegisterValue.Timestamp, 
              Value = ValueAndUnitMapper.Map(x.TimeRegisterValue.UnitValue.Value, unit), 
              DiffValue = diffValue != null ? ValueAndUnitMapper.Map(diffValue.Value, unit) : null, 
              Unit = ValueAndUnitMapper.Map(unit),
              DeviceId = x.TimeRegisterValue.SerialNumber
            };
          }).ToList()
        };
        exportSeries.Add(completeHourlyValues);
      }

      var r = new { Timestamps = hours, Series = exportSeries };
      return Response.AsJson(r);
    }

    private static double? CalculateDiffValue(NormalizedTimeRegisterValue value, NormalizedTimeRegisterValue previousValue)
    {
      if (previousValue.TimeRegisterValue.SerialNumberEquals(value.TimeRegisterValue) &&
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
