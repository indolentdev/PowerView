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
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IProfileRepository profileRepository;
    private readonly ISeriesColorRepository serieRepository;
    private readonly IProfileGraphRepository profileGraphRepository;
    private readonly ILocationContext locationContext;
    private readonly ISerieMapper serieMapper;

    public ProfileController(ILogger<ProfileController> logger, IProfileRepository profileRepository, ISeriesColorRepository serieRepository, IProfileGraphRepository profileGraphRepository, ILocationContext locationContext, ISerieMapper serieMapper)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        this.serieRepository = serieRepository ?? throw new ArgumentNullException(nameof(serieRepository));
        this.profileGraphRepository = profileGraphRepository ?? throw new ArgumentNullException(nameof(profileGraphRepository));
        this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
        this.serieMapper = serieMapper ?? throw new ArgumentNullException(nameof(serieMapper));
    }

    [HttpGet("day")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetDayProfile([BindRequired] string page, [BindRequired] string start)
    {
        return GetProfile(page, start, profileRepository.GetDayProfileSet, "day");
    }

    [HttpGet("month")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetMonthProfile([BindRequired] string page, [BindRequired] string start)
    {
        return GetProfile(page, start, profileRepository.GetMonthProfileSet, "month");
    }

    [HttpGet("year")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetYearProfile([BindRequired] string page, [BindRequired] string start)
    {
        return GetProfile(page, start, profileRepository.GetYearProfileSet, "year");
    }

    private ActionResult GetProfile(string page, string startString,
      Func<DateTime, DateTime, DateTime, TimeRegisterValueLabelSeriesSet> getLabelSeriesSet, string period)
    {
        DateTime start;
        if (!DateTime.TryParse(startString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out start) || start.Kind != DateTimeKind.Utc)
        {
            return BadRequest($"Unable to parse UTC start date time string:{startString}");
        }

        var profileGraphs = profileGraphRepository.GetProfileGraphs(period, page);
        if (profileGraphs.Count == 0 || profileGraphs.Any(x => x.SerieNames.Count == 0))
        {
            return BadRequest();
        }

        var viewSet = GetProfileViewSet(profileGraphs, getLabelSeriesSet, start, period);

        var r = new
        {
            Page = page,
            StartTime = DateTimeMapper.Map(start),
            Graphs = viewSet.SerieSets.Select(GetGraph).ToList(),
            PeriodTotals = viewSet.PeriodTotals.Select(GetPeriodTotal).ToList()
        };
        return Ok(r);
    }

    private ProfileViewSet GetProfileViewSet(ICollection<ProfileGraph> profileGraphs, Func<DateTime, DateTime, DateTime, TimeRegisterValueLabelSeriesSet> getLabelSeriesSet, DateTime start, string period)
    {
        // Distinct intervals
        var distinctIntervals = profileGraphs.GroupBy(x => x.Interval).ToList();

        // Find query start and end times based on max interval and period...
        var timeZoneInfo = locationContext.TimeZoneInfo;
        var dateTimeHelper = new DateTimeHelper(timeZoneInfo, start);
        var end = dateTimeHelper.GetPeriodEnd(period);
        var maxInterval = distinctIntervals.Select(x => dateTimeHelper.GetNext(x.Key)(start)).Max();
        var preStart = start.AddTicks((start - maxInterval).Ticks / 2); // .. half the interval backwards.

        // Query db
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        var labelSeriesSet = getLabelSeriesSet(preStart, start, end);
        sw.Stop();
        logger.LogDebug($"GetProfile timing - GetLabelSeriesSet: {sw.ElapsedMilliseconds}ms");

        // group by interval and generate additional series
        var intervalGroups = new List<ProfileGraphIntervalGroup>(distinctIntervals.Count);
        sw.Restart();
        foreach (var group in distinctIntervals)
        {
            var groupInterval = group.Key;
            var groupProfileGraphs = group.ToList();

            var intervalGroup = new ProfileGraphIntervalGroup(timeZoneInfo, start, groupInterval, groupProfileGraphs, labelSeriesSet);
            intervalGroup.Prepare();
            intervalGroups.Add(intervalGroup);
        }
        sw.Stop();
        logger.LogDebug($"GetProfile timing - Group by intervals and generate: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        var profileViewSetSource = new ProfileViewSetSource(profileGraphs, intervalGroups);
        var viewSet = profileViewSetSource.GetProfileViewSet();
        sw.Stop();
        logger.LogDebug($"GetProfile timing - GetProfileViewSet: {sw.ElapsedMilliseconds}ms");

        return viewSet;
    }

    private object GetGraph(SeriesSet serieSet)
    {
        var series = serieSet.Series.Select(x => new
        {
            x.SeriesName.Label,
            ObisCode = x.SeriesName.ObisCode.ToString(),
            Unit = ValueAndUnitMapper.Map(x.Unit),
            SerieType = serieMapper.MapToSerieType(x.SeriesName.ObisCode),
            SerieYAxis = serieMapper.MapToSerieYAxis(x.SeriesName.ObisCode),
            SerieColor = serieRepository.GetColorCached(x.SeriesName.Label, x.SeriesName.ObisCode),
            Values = x.Values.Select(value => ValueAndUnitMapper.Map(value, x.Unit)).ToList()
        });

        return new
        {
            Title = serieSet.Title,
            Categories = serieSet.Categories.Select(DateTimeMapper.Map).ToList(),
            Series = series.OrderBy(x => x.Label + x.ObisCode).ToList()
        };
    }

    private object GetPeriodTotal(NamedValue maxValue)
    {
        return new
        {
            maxValue.SerieName.Label,
            ObisCode = maxValue.SerieName.ObisCode.ToString(),
            Value = ValueAndUnitMapper.Map(maxValue.UnitValue.Value, maxValue.UnitValue.Unit),
            Unit = ValueAndUnitMapper.Map(maxValue.UnitValue.Unit)
        };
    }

}
