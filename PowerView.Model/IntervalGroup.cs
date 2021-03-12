using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerView.Model
{
  public class IntervalGroup
  {
    private readonly Func<DateTime, DateTime> timeDivider;
    private readonly Func<DateTime, DateTime> getNext;

    public IntervalGroup(TimeZoneInfo timeZoneinfo, DateTime start, string interval, TimeRegisterValueLabelSeriesSet labelSeriesSet)
    {
      var dateTimeHelper = new DateTimeHelper(timeZoneinfo, start);
      timeDivider = dateTimeHelper.GetDivider(interval);
      getNext = dateTimeHelper.GetNext(interval);
      if (labelSeriesSet == null) throw new ArgumentNullException("labelSeriesSet");

      Interval = interval;
      LabelSeriesSet = labelSeriesSet;
    }

    public string Interval { get; private set; }
    public TimeRegisterValueLabelSeriesSet LabelSeriesSet { get; private set; }

    public IList<DateTime> Categories { get; private set; }
    public LabelSeriesSet<NormalizedTimeRegisterValue> NormalizedLabelSeriesSet { get; private set; }
    public LabelSeriesSet<NormalizedDurationRegisterValue> NormalizedDurationLabelSeriesSet { get; private set; }

    public void Prepare()
    {
      Categories = new ReadOnlyCollection<DateTime>(GetCategories());

      NormalizedLabelSeriesSet = LabelSeriesSet.Normalize(timeDivider);

      NormalizedDurationLabelSeriesSet = new LabelSeriesSet<NormalizedDurationRegisterValue>(NormalizedLabelSeriesSet.Start, NormalizedLabelSeriesSet.End,
        TransformToNormalizedDurations(NormalizedLabelSeriesSet).ToList());

      // TODO: Remove this once nobody is using "duration normalized" values from the NormalizedLablSeriesSet.
      foreach (var labelSeries in NormalizedLabelSeriesSet)
      {
        var generator = new SeriesFromCumulativeGenerator();
        labelSeries.Add(generator.GenerateOld(labelSeries.GetCumulativeSeries()));
      }
    }

    private List<DateTime> GetCategories()
    {
      var categories = new List<DateTime>();
      var categoryTimestamp = LabelSeriesSet.Start;
      while (categoryTimestamp < LabelSeriesSet.End)
      {
        categories.Add(categoryTimestamp);
        categoryTimestamp = getNext(categoryTimestamp);
      }

      return categories;
    }

    private IEnumerable<LabelSeries<NormalizedDurationRegisterValue>> TransformToNormalizedDurations(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
    {
      foreach (var labelSeries in labelSeriesSet)
      {
        var durationLabelSeriesContent = new Dictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>>();

        var nonCumulativeLabelSeries = labelSeries.GetNonCumulativeSeries();
        foreach (var item in nonCumulativeLabelSeries)
        {
          var durationValues = item.Value.Select(x => new NormalizedDurationRegisterValue(x.TimeRegisterValue.Timestamp, x.TimeRegisterValue.Timestamp,
            x.NormalizedTimestamp, x.NormalizedTimestamp, x.TimeRegisterValue.UnitValue, x.TimeRegisterValue.DeviceId)).ToList();
          durationLabelSeriesContent.Add(item.Key, durationValues);
        }

        var generator = new SeriesFromCumulativeGenerator();
        var cumulativeLabelSeries = labelSeries.GetCumulativeSeries();
        if (cumulativeLabelSeries.Count > 0)
        {
          var durationValues = generator.Generate(cumulativeLabelSeries);
          foreach (var item in durationValues)
          {
            durationLabelSeriesContent.Add(item.Key, item.Value);
          }
        }

        var durationLabelSeries = new LabelSeries<NormalizedDurationRegisterValue>(labelSeries.Label, durationLabelSeriesContent);
        yield return durationLabelSeries;
      }
    }
  }
}
