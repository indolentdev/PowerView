﻿using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/gauges")]
public class GaugeController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IGaugeRepository gaugeRepository;

    public GaugeController(ILogger<GaugeController> logger, IGaugeRepository gaugeRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.gaugeRepository = gaugeRepository ?? throw new ArgumentNullException(nameof(gaugeRepository));
    }

    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetLatestGauges([FromQuery, StringLength(60, MinimumLength = 1)] string timestamp)
    {
        var timestampDateTime = GetTimestamp(timestamp);

        var gauges = gaugeRepository.GetLatest(timestampDateTime);

        var r = new
        {
            Timestamp = timestampDateTime.ToString("o"),
            Groups = gauges.Select(MapGaugeValueSet).ToArray()
        };
        return Ok(r);
    }

    [HttpGet("custom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetCustomGauges([FromQuery, StringLength(60, MinimumLength = 1)] string timestamp)
    {
        var timestampDateTime = GetTimestamp(timestamp);

        var gauges = gaugeRepository.GetCustom(timestampDateTime);

        var r = new
        {
            Timestamp = timestampDateTime.ToString("o"),
            Groups = gauges.Select(MapGaugeValueSet).ToArray()
        };
        return Ok(r);
    }

    private DateTime GetTimestamp(string timestamp)
    {
        DateTime timestampDateTime = DateTime.UtcNow;
        if (timestamp != null)
        {
            DateTime timestampParse;
            if (!DateTime.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out timestampParse) || timestampParse.Kind != DateTimeKind.Utc)
            {
                logger.LogInformation($"Unable to parse UTC timestamp date time string:{timestamp}");
            }
            else
            {
                timestampDateTime = timestampParse;
            }
        }
        return timestampDateTime;
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
            gaugeValue.Label,
            gaugeValue.DeviceId,
            Timestamp = DateTimeMapper.Map(gaugeValue.DateTime),
            ObisCode = gaugeValue.ObisCode.ToString(),
            Value = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Value, gaugeValue.UnitValue.Unit),
            Unit = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Unit)
        };
    }

}
