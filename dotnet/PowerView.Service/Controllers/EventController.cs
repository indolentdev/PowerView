﻿using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
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
                Value = ValueAndUnitMapper.Map(leakAmplification.UnitValue.Value, leakAmplification.UnitValue.Unit, reduceUnit),
                Unit = ValueAndUnitMapper.Map(leakAmplification.UnitValue.Unit, reduceUnit)
            };
        }

        return new { };
    }
}
