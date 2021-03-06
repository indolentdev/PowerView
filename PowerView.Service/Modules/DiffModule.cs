﻿using System;
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
  public class DiffModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IProfileRepository profileRepository;
    private readonly ILocationContext locationContext;

    public DiffModule(IProfileRepository profileRepository, ILocationContext locationContext)
      : base("/api")
    {
      if (profileRepository == null) throw new ArgumentNullException("profileRepository");
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.profileRepository = profileRepository;
      this.locationContext = locationContext;

      Get["diff"] = GetDiff;
    }

    private dynamic GetDiff(dynamic param)
    {
      var fromDate = GetDateTime("from");
      var toDate = GetDateTime("to");

      if (fromDate == null || toDate == null)
      {
        return HttpStatusCode.BadRequest;
      }

      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      var lss = profileRepository.GetMonthProfileSet(fromDate.Value.AddHours(-12), fromDate.Value, toDate.Value);
      sw.Stop();
      if (log.IsDebugEnabled) log.DebugFormat("GetDiff timing - Get: {0}ms", sw.ElapsedMilliseconds);

      var intervalGroup = new IntervalGroup(locationContext.TimeZoneInfo, fromDate.Value, "1-days", lss);
      sw.Restart();
      intervalGroup.Prepare();
      sw.Stop();
      if (log.IsDebugEnabled) log.DebugFormat("GetDiff timing - Normalize, generate: {0}ms", sw.ElapsedMilliseconds);

      var registers = MapItems(intervalGroup.NormalizedDurationLabelSeriesSet).ToList();
      var r = new {
        From = fromDate.Value.ToString("o"),
        To = toDate.Value.ToString("o"),
        Registers = registers
      };
 
      return Response.AsJson(r);
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

    private static IEnumerable<object> MapItems(LabelSeriesSet<NormalizedDurationRegisterValue> labelSeriesSet)
    {
      foreach (var labelSeries in labelSeriesSet)
      {
        var periodObisCodes = labelSeries.Where(oc => oc.IsPeriod).ToList();
        foreach (var obisCode in periodObisCodes)
        {
          var normalizedDurationRegisterValues = labelSeries[obisCode];
          if (normalizedDurationRegisterValues.Count < 2) continue;
          var last = normalizedDurationRegisterValues.Last();

          yield return new { labelSeries.Label, ObisCode=obisCode.ToString(),
            From = last.Start.ToString("o"), To = last.End.ToString("o"), 
            Value = ValueAndUnitMapper.Map(last.UnitValue.Value, last.UnitValue.Unit),
            Unit = ValueAndUnitMapper.Map(last.UnitValue.Unit) };
        }
      }
    }

    private static object MapGaugeValueSet(GaugeValueSet gaugeValueSet)
    {
      return new
      {
        Name = gaugeValueSet.Name.ToString(),
        Registers = gaugeValueSet.GaugeValues.Select(MapGaugeValue).ToArray()
      };
    }

    private static object MapGaugeValue(GaugeValue gaugeValue)
    {
      return new
      {
        gaugeValue.Label, gaugeValue.DeviceId, 
        Timestamp = DateTimeMapper.Map(gaugeValue.DateTime), 
        ObisCode = gaugeValue.ObisCode.ToString(),
        Value = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Value, gaugeValue.UnitValue.Unit),
        Unit = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Unit)
      };
    }

  }
}
