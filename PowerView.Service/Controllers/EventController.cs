using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase
{
    private readonly IMeterEventRepository meterEventRepository;

    public EventController(IMeterEventRepository meterEventRepository)
    {
        this.meterEventRepository = meterEventRepository ?? throw new ArgumentNullException(nameof(meterEventRepository));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetMeterEvents()
    {
        var meterEvents = meterEventRepository.GetMeterEvents();

        var events = new
        {
            TotalCount = meterEvents.TotalCount,
            Items = meterEvents.Result.Select(MapEvent).ToArray()
        };

        return Ok(events);
    }

    private object MapEvent(MeterEvent meterEvent)
    {
        var e = new
        {
            Type = meterEvent.Amplification.GetMeterEventType(),
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
            return new
            {
                StartTimestamp = leakAmplification.Start,
                EndTimestamp = leakAmplification.End,
                Value = ValueAndUnitConverter.Convert(leakAmplification.UnitValue.Value, leakAmplification.UnitValue.Unit, reduceUnit),
                Unit = ValueAndUnitConverter.Convert(leakAmplification.UnitValue.Unit, reduceUnit)
            };
        }

        return new { };
    }
}
