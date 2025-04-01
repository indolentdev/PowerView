using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/diff")]
public class DiffController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IProfileRepository profileRepository;
    private readonly ILocationContext locationContext;

    public DiffController(ILogger<DiffController> logger, IProfileRepository profileRepository, ILocationContext locationContext)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult GetDiff(
        [BindRequired, FromQuery, UtcDateTime] DateTime from,
        [BindRequired, FromQuery, UtcDateTime] DateTime to)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        var lss = profileRepository.GetMonthProfileSet(from.AddHours(-12), from, to);
        sw.Stop();
        if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("GetDiff timing - Get: {Elapsed}ms", sw.ElapsedMilliseconds);

        var intervalGroup = new IntervalGroup(locationContext, from, "1-days", lss, Array.Empty<CostBreakdownGeneratorSeries>());
        sw.Restart();
        intervalGroup.Prepare();
        sw.Stop();
        if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("GetDiff timing - Normalize, generate: {Elapsed}ms", sw.ElapsedMilliseconds);

        var registers = MapItems(intervalGroup.NormalizedDurationLabelSeriesSet).ToList();
        var r = new
        {
            From = from.ToString("o"),
            To = to.ToString("o"),
            Registers = registers
        };
        return Ok(r);
    }

    private DateTime? GetDateTime(string queryParamName, string queryParamValue)
    {
        DateTime timestampParse;
        if (!DateTime.TryParse(queryParamValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out timestampParse) || timestampParse.Kind != DateTimeKind.Utc)
        {
            logger.LogInformation("Unable to parse query parameter {Name} as UTC timestamp date time string:{Value}", queryParamName, queryParamValue);
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

                yield return new
                {
                    labelSeries.Label,
                    ObisCode = obisCode.ToString(),
                    From = last.Start.ToString("o"),
                    To = last.End.ToString("o"),
                    Value = ValueAndUnitConverter.Convert(last.UnitValue.Value, last.UnitValue.Unit),
                    Unit = ValueAndUnitConverter.Convert(last.UnitValue.Unit)
                };
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
            gaugeValue.Label,
            gaugeValue.DeviceId,
            Timestamp = DateTimeMapper.Map(gaugeValue.DateTime),
            ObisCode = gaugeValue.ObisCode.ToString(),
            Value = ValueAndUnitConverter.Convert(gaugeValue.UnitValue.Value, gaugeValue.UnitValue.Unit),
            Unit = ValueAndUnitConverter.Convert(gaugeValue.UnitValue.Unit)
        };
    }

}
