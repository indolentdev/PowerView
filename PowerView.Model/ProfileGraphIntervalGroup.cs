using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerView.Model
{
  public class ProfileGraphIntervalGroup
  {
    private readonly Func<DateTime, DateTime> timeDivider;
    private readonly Func<DateTime, DateTime> getNext;

    public ProfileGraphIntervalGroup(TimeZoneInfo timeZoneinfo, DateTime start, string interval, IList<ProfileGraph> profileGraphs, LabelSeriesSet<TimeRegisterValue> labelSeriesSet)
    {
      var dateTimeHelper = new DateTimeHelper(timeZoneinfo, start);
      timeDivider = dateTimeHelper.GetDivider(interval);
      getNext = dateTimeHelper.GetNext(interval);
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");
      if (labelSeriesSet == null) throw new ArgumentNullException("labelSeriesSet");

      Interval = interval;
      ProfileGraphs = new ReadOnlyCollection<ProfileGraph>(profileGraphs);
      LabelSeriesSet = labelSeriesSet;
    }

    public string Interval { get; private set; }
    public ICollection<ProfileGraph> ProfileGraphs { get; private set; }
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
