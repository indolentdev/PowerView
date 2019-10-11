using System;
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
  public class GaugeModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IGaugeRepository gaugeRepository;

    public GaugeModule(IGaugeRepository gaugeRepository)
      : base("/api/gauges")
    {
      if (gaugeRepository == null) throw new ArgumentNullException("gaugeRepository");

      this.gaugeRepository = gaugeRepository;

      Get["latest"] = GetLatestGauges;
      Get["custom"] = GetCustomGauges;
    }

    private dynamic GetLatestGauges(dynamic param)
    {
      var timestamp = GetTimestamp();

      var gauges = gaugeRepository.GetLatest(timestamp);

      var r = new
      {
        Timestamp = timestamp.ToString("o"),
        Groups = gauges.Select(MapGaugeValueSet).ToArray()
      };

      return Response.AsJson(r);
    }

    private dynamic GetCustomGauges(dynamic param)
    {
      var timestamp = GetTimestamp();

      var gauges = gaugeRepository.GetCustom(timestamp);

      var r = new
      {
        Timestamp = timestamp.ToString("o"),
        Groups = gauges.Select(MapGaugeValueSet).ToArray()
      };

      return Response.AsJson(r);
    }

    private DateTime GetTimestamp()
    {
      DateTime timestamp = DateTime.UtcNow;
      if (Request.Query.timestamp.HasValue)
      {
        DateTime timestampParse;
        string timestampString = Request.Query.timestamp;
        if (!DateTime.TryParse(timestampString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out timestampParse) ||
        timestampParse.Kind != DateTimeKind.Utc)
        {
          log.InfoFormat("Unable to parse UTC timestamp date time string:{0}", timestampString);
        }
        else
        {
          timestamp = timestampParse;
        }
      }
      return timestamp;
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
