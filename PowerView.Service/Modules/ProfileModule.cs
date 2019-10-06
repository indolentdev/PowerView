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
  public class ProfileModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IProfileRepository profileRepository;
    private readonly ISeriesColorRepository serieRepository;
    private readonly IProfileGraphRepository profileGraphRepository;
    private readonly ILocationProvider locationProvider;
    private readonly ISerieMapper serieMapper;
    private readonly ITemplateConfigProvider templateConfigProvider;

    public ProfileModule(IProfileRepository profileRepository, ISeriesColorRepository serieRepository, IProfileGraphRepository profileGraphRepository, ILocationProvider locationProvider, ISerieMapper serieMapper, ITemplateConfigProvider templateConfigProvider)
      :base("/api/profile")
    {
      if (profileRepository == null) throw new ArgumentNullException("profileRepository");
      if (serieRepository == null) throw new ArgumentNullException("serieRepository");
      if (profileGraphRepository == null) throw new ArgumentNullException("profileGraphRepository");
      if (locationProvider == null) throw new ArgumentNullException("locationProvider");
      if (serieMapper == null) throw new ArgumentNullException("serieMapper");
      if (templateConfigProvider == null) throw new ArgumentNullException("templateConfigProvider");

      this.profileRepository = profileRepository;
      this.serieRepository = serieRepository;
      this.profileGraphRepository = profileGraphRepository;
      this.locationProvider = locationProvider;
      this.serieMapper = serieMapper;
      this.templateConfigProvider = templateConfigProvider;

      Get["day"] = GetDayProfile;
      Get["month"] = GetMonthProfile;
      Get["year"] = GetYearProfile;
    }

    private dynamic GetDayProfile(dynamic param)
    {
      return GetProfile(profileRepository.GetDayProfileSet, "day");
    }

    private dynamic GetMonthProfile(dynamic param)
    {
      return GetProfile(profileRepository.GetMonthProfileSet, "month");
    }

    private dynamic GetYearProfile(dynamic param)
    {
      return GetProfile(profileRepository.GetYearProfileSet, "year");
    }

    private dynamic GetProfile(Func<DateTime, DateTime, DateTime, LabelSeriesSet<TimeRegisterValue>> getLabelSeriesSet, string period)
    {
      if (!Request.Query.page.HasValue)
      {
        return Response.AsJson("Query parameter page is misisng.", HttpStatusCode.BadRequest);
      }
      if (!Request.Query.start.HasValue)
      {
        return Response.AsJson("Query parameter start is missing.", HttpStatusCode.BadRequest);
      }
      string page = Request.Query.page;
      string startString = Request.Query.start;
      DateTime start;
      if (!DateTime.TryParse(startString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out start) ||
        start.Kind != DateTimeKind.Utc)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Unable to parse UTC start date time string:{0}", startString);
        return Response.AsJson(msg, HttpStatusCode.BadRequest);
      }

      var profileGraphs = profileGraphRepository.GetProfileGraphs(period, page);
      if (profileGraphs.Count == 0 || profileGraphs.Any(x => x.SerieNames.Count == 0))
      {
        return HttpStatusCode.BadRequest;
      }

      var viewSet = GetProfileViewSet(profileGraphs, getLabelSeriesSet, start, period);

      var r = new
      {
        Page = page,
        StartTime = DateTimeMapper.Map(start),
        Graphs = viewSet.SerieSets.Select(GetGraph).ToList(),
        PeriodTotals = viewSet.PeriodTotals.Select(GetPeriodTotal).ToList()
      };

      return Response.AsJson(r);
    }

    private ProfileViewSet GetProfileViewSet(ICollection<ProfileGraph> profileGraphs, Func<DateTime, DateTime, DateTime, LabelSeriesSet<TimeRegisterValue>> getLabelSeriesSet, DateTime start, string period)
    {
      // Distinct intervals
      var distinctIntervals = profileGraphs.GroupBy(x => x.Interval).ToList();

      // Find query start and end times based on max interval and period...
      var timeZoneInfo = locationProvider.GetTimeZone();
      var dateTimeHelper = new DateTimeHelper(timeZoneInfo, start);
      var end = dateTimeHelper.GetPeriodEnd(period);
      var maxInterval = distinctIntervals.Select(x => dateTimeHelper.GetNext(x.Key)(start)).Max();
      var preStart = start.AddTicks((start - maxInterval).Ticks/2); // .. half the interval backwards.

      // Query db
      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      var labelSeriesSet = getLabelSeriesSet(preStart, start, end);
      sw.Stop();
      if (log.IsDebugEnabled) log.DebugFormat("GetProfile timing - GetLabelSeriesSet: {0}ms", sw.ElapsedMilliseconds);

      // group by interval and generate additional series
      var intervalGroups = new List<IntervalGroup>(distinctIntervals.Count);
      sw.Restart();
      foreach (var group in distinctIntervals)
      {
        var groupInterval = group.Key;
        var groupProfileGraphs = group.ToList();

        var intervalGroup = new IntervalGroup(timeZoneInfo, start, groupInterval, groupProfileGraphs, labelSeriesSet);
        intervalGroup.Prepare(templateConfigProvider.LabelObisCodeTemplates);
        intervalGroups.Add(intervalGroup);
      }
      sw.Stop();
      if (log.IsDebugEnabled) log.DebugFormat("GetProfile timing - Group by intervals and generate: {0}ms", sw.ElapsedMilliseconds);

      sw.Restart();
      var profileViewSetSource = new ProfileViewSetSource(profileGraphs, intervalGroups);
      var viewSet = profileViewSetSource.GetProfileViewSet();
      sw.Stop();
      if (log.IsDebugEnabled) log.DebugFormat("GetProfile timing - GetProfileViewSet: {0}ms", sw.ElapsedMilliseconds);

      return viewSet;
    }

    private object GetGraph(SeriesSet serieSet)
    {
      var series = serieSet.Series.Select(x => new {
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
      return new { maxValue.SerieName.Label, ObisCode=maxValue.SerieName.ObisCode.ToString(),
        Value=ValueAndUnitMapper.Map(maxValue.UnitValue.Value, maxValue.UnitValue.Unit),
        Unit=ValueAndUnitMapper.Map(maxValue.UnitValue.Unit) };
    }

  }
}
