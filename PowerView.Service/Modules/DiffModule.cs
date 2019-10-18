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
  public class DiffModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IProfileRepository profileRepository;
    private readonly ITemplateConfigProvider templateConfigProvider;
    private readonly ILocationContext locationContext;

    public DiffModule(IProfileRepository profileRepository, ITemplateConfigProvider templateConfigProvider, ILocationContext locationContext)
      : base("/api")
    {
      if (profileRepository == null) throw new ArgumentNullException("profileRepository");
      if (templateConfigProvider == null) throw new ArgumentNullException("templateConfigProvider");
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.profileRepository = profileRepository;
      this.templateConfigProvider = templateConfigProvider;
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

      var intervalGroup = new IntervalGroup(locationContext.TimeZoneInfo, fromDate.Value, "1-days", new ProfileGraph[0], lss);
      sw.Restart();
      intervalGroup.Prepare(templateConfigProvider.LabelObisCodeTemplates);
      sw.Stop();
      if (log.IsDebugEnabled) log.DebugFormat("GetDiff timing - Normalize, generate: {0}ms", sw.ElapsedMilliseconds);

      var registers = MapItems(intervalGroup.NormalizedLabelSeriesSet).ToList();
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

    private static IEnumerable<object> MapItems(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
    {
      foreach (var labelSeries in labelSeriesSet)
      {
        var periodObisCodes = labelSeries.Where(oc => oc.IsPeriod).ToList();
        foreach (var obisCode in periodObisCodes)
        {
          var normalizedTimeRegisterValues = labelSeries[obisCode];
          if (normalizedTimeRegisterValues.Count < 2) continue;
          var first = normalizedTimeRegisterValues.First();
          var last = normalizedTimeRegisterValues.Last();

          yield return new { labelSeries.Label, ObisCode=obisCode.ToString(),
            From = first.TimeRegisterValue.Timestamp.ToString("o"), To = last.TimeRegisterValue.Timestamp.ToString("o"), 
            Value = ValueAndUnitMapper.Map(last.TimeRegisterValue.UnitValue.Value, last.TimeRegisterValue.UnitValue.Unit),
            Unit = ValueAndUnitMapper.Map(last.TimeRegisterValue.UnitValue.Unit) };
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
        gaugeValue.Label, gaugeValue.SerialNumber, 
        Timestamp = DateTimeMapper.Map(gaugeValue.DateTime), 
        ObisCode = gaugeValue.ObisCode.ToString(),
        Value = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Value, gaugeValue.UnitValue.Unit),
        Unit = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Unit)
      };
    }

  }
}
