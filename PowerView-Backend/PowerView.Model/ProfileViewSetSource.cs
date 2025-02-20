using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class ProfileViewSetSource
  {
    private readonly ICollection<ProfileGraph> profileGraphs;
    private readonly IDictionary<string, IList<DateTime>> intervalToCategories;
    private readonly IDictionary<string, IDictionary<SeriesName, IEnumerable<NormalizedDurationRegisterValue>>> intervalSeriesNameToValues;

    public ProfileViewSetSource(IEnumerable<ProfileGraph> profileGraphs, IList<ProfileGraphIntervalGroup> intervalGroups)
    {
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");
      if (intervalGroups == null) throw new ArgumentNullException("intervalGroups");

      this.profileGraphs = profileGraphs.ToList();

      var groupProfileGraphs = intervalGroups.SelectMany(x => x.ProfileGraphs).ToList();
      if (this.profileGraphs.Count != groupProfileGraphs.Count)
      {
        throw new ArgumentOutOfRangeException("intervalGroups", "Must contain all instances of profileGraphs argument");
      }
      foreach (var profileGraph in profileGraphs)
      {
        if (!groupProfileGraphs.Contains(profileGraph))
        {
          throw new ArgumentOutOfRangeException("intervalGroups", "Must contain all instances of profileGraphs argument");
        }
      }

      intervalToCategories = intervalGroups.ToDictionary(x => x.Interval, x => x.Categories);
      intervalSeriesNameToValues = intervalGroups.ToDictionary(x => x.Interval, GetSeriesNamesAndValues);
    }

    private static IDictionary<SeriesName, IEnumerable<NormalizedDurationRegisterValue>> GetSeriesNamesAndValues(ProfileGraphIntervalGroup intervalGroup)
    {
      var result = new Dictionary<SeriesName, IEnumerable<NormalizedDurationRegisterValue>>();

      var labelSeriesSet = intervalGroup.NormalizedDurationLabelSeriesSet;
      foreach (var labelSeries in labelSeriesSet)
      {
        foreach (var obisCode in labelSeries)
        {
          var seriesName = new SeriesName(labelSeries.Label, obisCode);
          var normalizedTimeRegisterValues = labelSeries[obisCode]
            .Where(x => x.End >= labelSeriesSet.Start && x.End < labelSeriesSet.End)
            .Select(x => x);
          result.Add(seriesName, normalizedTimeRegisterValues);
        }
      }

      return result;
    }

    public ProfileViewSet GetProfileViewSet()
    {
      var seriesSets = new List<SeriesSet>(profileGraphs.Count);
      foreach (var profileGraph in profileGraphs)
      {
        var categories = intervalToCategories[profileGraph.Interval];

        var profileGraphSeries = new List<Series>(profileGraph.SerieNames.Count);
        foreach (var seriesName in profileGraph.SerieNames)
        {
          if (!intervalSeriesNameToValues.ContainsKey(profileGraph.Interval) || !intervalSeriesNameToValues[profileGraph.Interval].ContainsKey(seriesName))
          {
            continue;
          }

          var normalizedDurationRegisterValues = intervalSeriesNameToValues[profileGraph.Interval][seriesName];
          var values = 
                            (from categoryTimestamp in categories
                             join normalizedTimeRegisterValue in normalizedDurationRegisterValues
                             on categoryTimestamp equals normalizedTimeRegisterValue.NormalizedEnd 
                             into items
                             from timeRegisterValueOrNull in items.DefaultIfEmpty()
                             select timeRegisterValueOrNull)
                             .ToList();
          var firstValue = values.FirstOrDefault(x => x != null);
          if (firstValue == null) continue;

          var valuesForCategories = values.Select(x => x == null ? null : (double?)x.UnitValue.Value);
          var series = new Series(seriesName, firstValue.UnitValue.Unit, valuesForCategories);
          profileGraphSeries.Add(series);
        }

        if (profileGraphSeries.Count == 0) continue;



        var seriesSet = new SeriesSet(profileGraph.Title, categories, profileGraphSeries);
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
