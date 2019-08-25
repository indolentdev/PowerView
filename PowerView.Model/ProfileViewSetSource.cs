using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class ProfileViewSetSource
  {
    private readonly ICollection<ProfileGraph> profileGraphs;
    private readonly IDictionary<string, IntervalGroup> intervalGroups;

    public ProfileViewSetSource(IEnumerable<ProfileGraph> profileGraphs, IList<IntervalGroup> intervalGroups)
    {
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");
      if (intervalGroups == null) throw new ArgumentNullException("intervalGroups");

      this.profileGraphs = profileGraphs.ToList();
      this.intervalGroups = intervalGroups.ToDictionary(x => x.Interval, x => x);

      if (this.profileGraphs.Count != this.intervalGroups.Values.SelectMany(x => x.ProfileGraphs).Count())
      {
        throw new ArgumentOutOfRangeException("intervalGroups", "Must contain all instances of profileGraphs argument");
      }
      foreach (var profileGraph in profileGraphs)
      {
        if (!this.intervalGroups.ContainsKey(profileGraph.Interval) || !this.intervalGroups[profileGraph.Interval].ProfileGraphs.Contains(profileGraph))
        {
          throw new ArgumentOutOfRangeException("intervalGroups", "Must contain all instances of profileGraphs argument");
        }
      }
    }

    public ProfileViewSet GetProfileViewSet()
    {
      // TODO: slice labelseriessets to start time..

      var seriesSets = new List<SeriesSet>(profileGraphs.Count);
      foreach (var profileGraph in profileGraphs)
      {
        var intervalGroup = intervalGroups[profileGraph.Interval];

        var profileGraphSeries = new List<Series>(profileGraph.SerieNames.Count);
        foreach (var seriesName in profileGraph.SerieNames)
        { 
          var labelSeriesByLabel = intervalGroup.PreparedLabelSeriesSet.ToDictionary(x => x.Label, x => x);
          if (!labelSeriesByLabel.ContainsKey(seriesName.Label)) continue;
          var labelSeries = labelSeriesByLabel[seriesName.Label];
          if (!labelSeries.ContainsObisCode(seriesName.ObisCode)) continue;

          var timeRegisterValues = 
                            (from categoryTimestamp in intervalGroup.Categories
                             join timeRegisterValue in labelSeries[seriesName.ObisCode].Select(x => (TimeRegisterValue?)x)
                             on categoryTimestamp equals timeRegisterValue.Value.Timestamp 
                             into items
                             from timeRegisterValueOrNull in items.DefaultIfEmpty()
                             select timeRegisterValueOrNull)
                             .ToList();
          var firstTimeRegisterValue = timeRegisterValues.FirstOrDefault(x => x != null);
          if (firstTimeRegisterValue == null) continue;

          var valuesForCategories = timeRegisterValues.Select(x => x == null ? null : (double?)x.Value.UnitValue.Value);
          var series = new Series(seriesName, firstTimeRegisterValue.Value.UnitValue.Unit, valuesForCategories);
          profileGraphSeries.Add(series);
        }

        if (profileGraphSeries.Count == 0) continue;

        var seriesSet = new SeriesSet(profileGraph.Title, intervalGroup.Categories, profileGraphSeries);
        seriesSets.Add(seriesSet);
      }

      // TODO: Consider from which profile graph to pick the series for period totals.. they may not be the same....
      var periodTotals = seriesSets.SelectMany(x => x.Series)
                                  .Where(x => x.SeriesName.ObisCode.IsPeriod)
                                  .GroupBy(x => x.SeriesName)
                                  .Select(x => x.First())
                                  .Select(x => new NamedValue(x.SeriesName, new UnitValue((double)x.Values.Reverse().First(z => z != null), x.Unit)))
                                  .ToList();

      var profileViewSet = new ProfileViewSet(seriesSets, periodTotals);
      return profileViewSet;
    }

  }
}
