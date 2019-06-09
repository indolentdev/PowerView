using System;
using System.Linq;
using Nancy;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Modules
{
  public class EventModule : CommonNancyModule
  {
//    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IMeterEventRepository meterEventRepository;

    public EventModule(IMeterEventRepository meterEventRepository)
      : base("/api")
    {
      if (meterEventRepository == null) throw new ArgumentNullException("meterEventRepository");

      this.meterEventRepository = meterEventRepository;

      Get["events"] = GetMeterEvents;
    }

    private dynamic GetMeterEvents(dynamic param)
    {
      var meterEvents = meterEventRepository.GetMeterEvents();

      var events = new { TotalCount = meterEvents.TotalCount,
        Items = meterEvents.Result.Select(MapEvent).ToArray()
      };

      return Response.AsJson(events);
    }

    private object MapEvent(MeterEvent meterEvent)
    {
      var e = new {  Type = meterEvent.Amplification.GetMeterEventType(),
        Label = meterEvent.Label,
        DetectTimestamp = meterEvent.DetectTimestamp,
        Status = meterEvent.Flag,
        Amplification = MapAmplification(meterEvent.Amplification)
      };
      return e;
    }

    private object MapAmplification(IMeterEventAmplification amplification)
    {
      const bool reduceUnit = true;
      var leakAmplification = amplification as LeakMeterEventAmplification;
      if (leakAmplification != null) 
      {
        return new { StartTimestamp = leakAmplification.Start, EndTimestamp = leakAmplification.End,
          Value = ValueAndUnitMapper.Map(leakAmplification.UnitValue.Value, leakAmplification.UnitValue.Unit, reduceUnit),
          Unit = ValueAndUnitMapper.Map(leakAmplification.UnitValue.Unit, reduceUnit) };
      }

      return new { };
    }
  }
}
