using System.Globalization;
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
    public ActionResult GetDiff([BindRequired] string from, [BindRequired] string to)
    {
        var fromDate = GetDateTime("from", from);
        var toDate = GetDateTime("to", to);

        if (fromDate == null || toDate == null)
        {
            return BadRequest();
        }

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        var lss = profileRepository.GetMonthProfileSet(fromDate.Value.AddHours(-12), fromDate.Value, toDate.Value);
        sw.Stop();
        if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug($"GetDiff timing - Get: {sw.ElapsedMilliseconds}ms");

        var intervalGroup = new IntervalGroup(locationContext.TimeZoneInfo, fromDate.Value, "1-days", lss);
        sw.Restart();
        intervalGroup.Prepare();
        sw.Stop();
        if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug($"GetDiff timing - Normalize, generate: {sw.ElapsedMilliseconds}ms");

        var registers = MapItems(intervalGroup.NormalizedDurationLabelSeriesSet).ToList();
        var r = new
        {
            From = fromDate.Value.ToString("o"),
            To = toDate.Value.ToString("o"),
            Registers = registers
        };
        return Ok(r);
    }

    private DateTime? GetDateTime(string queryParamName, string queryParamValue)
    {
        DateTime timestampParse;
        if (!DateTime.TryParse(queryParamValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out timestampParse) || timestampParse.Kind != DateTimeKind.Utc)
        {
            logger.LogInformation($"Unable to parse query parameter {queryParamName} as UTC timestamp date time string:{queryParamValue}");
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
                    Value = ValueAndUnitMapper.Map(last.UnitValue.Value, last.UnitValue.Unit),
                    Unit = ValueAndUnitMapper.Map(last.UnitValue.Unit)
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
            Value = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Value, gaugeValue.UnitValue.Unit),
            Unit = ValueAndUnitMapper.Map(gaugeValue.UnitValue.Unit)
        };
    }

}
