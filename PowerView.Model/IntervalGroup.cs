using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerView.Model
{
  public class IntervalGroup
  {
    private readonly Func<DateTime, DateTime> timeDivider;
    private readonly Func<DateTime, DateTime> getNext;

    public IntervalGroup(TimeZoneInfo timeZoneinfo, DateTime start, string interval, LabelSeriesSet<TimeRegisterValue> labelSeriesSet)
    {
      var dateTimeHelper = new DateTimeHelper(timeZoneinfo, start);
      timeDivider = dateTimeHelper.GetDivider(interval);
      getNext = dateTimeHelper.GetNext(interval);
      if (labelSeriesSet == null) throw new ArgumentNullException("labelSeriesSet");

      Interval = interval;
      LabelSeriesSet = labelSeriesSet;
    }

    public string Interval { get; private set; }
    public LabelSeriesSet<TimeRegisterValue> LabelSeriesSet { get; private set; }

    public IList<DateTime> Categories { get; private set; }
    public LabelSeriesSet<NormalizedTimeRegisterValue> NormalizedLabelSeriesSet { get; private set; }

    public void Prepare()
    {
      Categories = new ReadOnlyCollection<DateTime>(GetCategories());

      NormalizedLabelSeriesSet = LabelSeriesSet.Normalize(timeDivider);
      GenerateSeriesFromCumulative();
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

    private void GenerateSeriesFromCumulative()
    {
      foreach (var labelSeries in NormalizedLabelSeriesSet)
      {
        var generator = new SeriesFromCumulativeGenerator();
        labelSeries.Add(generator.Generate(labelSeries.GetCumulativeSeries()));
      }
    }

  }
}
