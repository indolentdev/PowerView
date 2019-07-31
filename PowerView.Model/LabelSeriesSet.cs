using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PowerView.Model.Expression;
using log4net;

namespace PowerView.Model
{
  public class LabelSeriesSet : IEnumerable<LabelSeries>
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly List<LabelSeries> labelSeries;

    public LabelSeriesSet(DateTime start, DateTime end, ICollection<LabelSeries> labelSeries)
      : this(start, end, labelSeries != null ? labelSeries.Count : 0, labelSeries)
    {
    }

    internal LabelSeriesSet(DateTime start, DateTime end, int estimatedLabelSeriesCount, IEnumerable<LabelSeries> labelSeries)
    {
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if (end.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("end", "Must be UTC");
      if (labelSeries == null) throw new ArgumentNullException("labelSeries");

      Start = start;
      End = end;

      this.labelSeries = new List<LabelSeries>(estimatedLabelSeriesCount);
      this.labelSeries.AddRange(labelSeries);
    }

    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    public LabelSeriesSet Normalize(Func<DateTime, DateTime> timeDivider)
    {
      if (timeDivider == null) throw new ArgumentNullException("timeDivider");

      return new LabelSeriesSet(Start, End, labelSeries.Count, labelSeries.Select(x => x.Normalize(timeDivider)));
    }

    /*
        public void GenerateFromTemplates(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates, string interval)
        {
          if (labelObisCodeTemplates == null) throw new ArgumentNullException("labelObisCodeTemplates");
          var timeDivider = DateTimeResolutionDivider.GetResolutionDivider(interval);

          var labelsAndObisCodes = GetLabelsAndObisCodes();

          foreach (var labelObisCodeTemplate in labelObisCodeTemplates)
          {
            var obisCodeAndValues = Generate(labelsAndObisCodes, labelObisCodeTemplate.Label, labelObisCodeTemplate.ObisCodeTemplates, timeDivider);
            if (obisCodeAndValues.Count == 0)
            {
              continue;
            }
            var labelProfile = new LabelProfile(labelObisCodeTemplate.Label, Start, obisCodeAndValues);
            labelProfiles.Add(labelProfile);
          }
        }
    */
    /*
        private IDictionary<string, ICollection<ObisCode>> GetLabelsAndObisCodes()
        {
          var result = new Dictionary<string, ICollection<ObisCode>>();

          foreach (var labelProfile in labelProfiles)
          {
            result.Add(labelProfile.Label.ToLowerInvariant(), labelProfile.GetAllObisCodes());
          }
          return result;
        }
    */
    /*
        private IDictionary<ObisCode, ICollection<TimeRegisterValue>> Generate(IDictionary<string, ICollection<ObisCode>> labelsAndObisCodes, string label, ICollection<ObisCodeTemplate> obisCodeTemplates, Func<DateTime, DateTime> timeDivider)
        {
          var obisCodeAndValues = new Dictionary<ObisCode, ICollection<TimeRegisterValue>>();
          foreach (var obisCodeTemplate in obisCodeTemplates)
          {
            if (!obisCodeTemplate.TemplateExpression.IsSatisfied(labelsAndObisCodes))
            {
              log.DebugFormat("Template has unmet data sources. Skipping it. Label:{0}, ObisCode:{1}", label, obisCodeTemplate.ObisCode);
              continue;
            }

            try
            {
              var valueExpressionSet = obisCodeTemplate.TemplateExpression.GetValueExpressionSet(this, timeDivider);
              var coarseTimeRegisterValues = valueExpressionSet.Evaluate();
              var timeRegisterValues = coarseTimeRegisterValues.Select(x => x.TimeRegisterValue).OrderBy(x => x.Timestamp).ToList();
              obisCodeAndValues.Add(obisCodeTemplate.ObisCode, timeRegisterValues);
            }
            catch (ValueExpressionSetException e)
            {
              var msg = string.Format(CultureInfo.InvariantCulture, "Failed calculation for template. Skipping it. Label:{0}, ObisCode:{1}", 
                                      label, obisCodeTemplate.ObisCode);
              log.Error(msg, e);
            }
          }
          return obisCodeAndValues;
        }
    */
    /*
        public ProfileViewSet GetProfileViewSet(ICollection<ProfileGraph> profileGraphs)
        {
          Func<string, Func<DateTime, DateTime>> timeDividerFactory = DateTimeResolutionDivider.GetResolutionDivider;

          var seriesByProfileGraph = GetSeriesByProfileGraph(timeDividerFactory, profileGraphs);

          var serieSets = new List<SeriesSet>(profileGraphs.Count);
          foreach (var profileGraphAndSeries in seriesByProfileGraph)
          {
            var profileGraph = profileGraphAndSeries.Key;
            var serieValuesBySerieName = profileGraphAndSeries.Value;

            var timeEntries = serieValuesBySerieName.Values.SelectMany(x => x).Select(x => x .Timestamp).Distinct().OrderBy(x => x).ToList();

            var series = new List<Series>(profileGraph.SerieNames.Count);
            foreach (var serieNameAndValues in serieValuesBySerieName)
            {
              var serieName = serieNameAndValues.Key;
              var serieCoarseTimestamps = serieNameAndValues.Value.GroupBy(x => x.Timestamp);

              var obisEntries = (from timestamp in timeEntries
                                 join readingGroup in serieCoarseTimestamps on timestamp equals readingGroup.Key into items
                                 from readingOrNull in items.DefaultIfEmpty()
                                 select (readingOrNull!=null ? readingOrNull.Last() : null)).ToArray();
              var entry = obisEntries.FirstOrDefault(x => x != null);
              if (entry == null)
              {
                continue;
              }

              var serie = new Series(serieName, entry.TimeRegisterValue.UnitValue.Unit,
                obisEntries.Select(x => (x == null ? null : (double?)x.TimeRegisterValue.UnitValue.Value)));
              series.Add(serie);
            }

            if (series.Count == 0)
            {
              continue;
            }

            var serieSet = new SeriesSet(profileGraph.Title, timeEntries, series);
            serieSets.Add(serieSet);
          }

          var periodTotals = serieSets.SelectMany(x => x.Series)
                                      .Where(x => x.SeriesName.ObisCode.IsPeriod)
                                      .GroupBy(x => x.SeriesName)
                                      .Select(x => x.First())
                                      .Select(x => new NamedValue(x.SeriesName, new UnitValue((double)x.Values.Reverse().First(z => z != null), x.Unit)))
                                      .ToList();

          var profileViewSet = new ProfileViewSet(serieSets, periodTotals);
          return profileViewSet;
        }
    */
    /*
        private IDictionary<ProfileGraph, IDictionary<SeriesName, ICollection<CoarseTimeRegisterValue>>> GetSeriesByProfileGraph(Func<string, Func<DateTime, DateTime>> timeDividerFactory, ICollection<ProfileGraph> profileGraphs)
        {
          var labelProfilesByLabel = labelProfiles.ToDictionary(x => x.Label, x => x);
          var graphs = new Dictionary<ProfileGraph, IDictionary<SeriesName, ICollection<CoarseTimeRegisterValue>>>(profileGraphs.Count);
          foreach (var profileGraph in profileGraphs)
          {
            var serieValues = new Dictionary<SeriesName, ICollection<CoarseTimeRegisterValue>>(profileGraph.SerieNames.Count);
            var timeDivider = timeDividerFactory(profileGraph.Interval);
            foreach (var serieName in profileGraph.SerieNames)
            {
              if (!labelProfilesByLabel.ContainsKey(serieName.Label)) continue;
              var labelProfile = labelProfilesByLabel[serieName.Label];
              if (!labelProfile.ContainsObisCode(serieName.ObisCode)) continue;
              serieValues.Add(serieName, labelProfile[serieName.ObisCode]
                              .Select(sv => new CoarseTimeRegisterValue(sv, timeDivider(sv.Timestamp)))
                              .OrderBy(x => x.TimeRegisterValue.Timestamp).ToList()
              );
            }
            graphs.Add(profileGraph, serieValues);
          }
          return graphs;
        }
    */
    #region IEnumerable implementation
    public IEnumerator<LabelSeries> GetEnumerator()
    {
      return labelSeries.GetEnumerator();
    }
    #endregion
    #region IEnumerable implementation
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
/*
    private class CoarseTimeRegisterValue
    {
      public CoarseTimeRegisterValue(TimeRegisterValue timeRegisterValue, DateTime timestamp)
      {
        TimeRegisterValue = timeRegisterValue;
        Timestamp = timestamp;
      }

      public TimeRegisterValue TimeRegisterValue { get; private set; }
      public DateTime Timestamp { get; private set; }
    }
*/    
  }

}
