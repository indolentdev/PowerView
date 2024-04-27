using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerView.Model
{
  public class ProfileGraphIntervalGroup : IntervalGroup
  {
    public ProfileGraphIntervalGroup(TimeZoneInfo timeZoneinfo, DateTime start, string interval, IList<ProfileGraph> profileGraphs, TimeRegisterValueLabelSeriesSet labelSeriesSet, IList<CostBreakdownGeneratorSeries> costBreakdownGeneratorSeries)
      : base(timeZoneinfo, start, interval, labelSeriesSet, costBreakdownGeneratorSeries)
    {
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");

      ProfileGraphs = new ReadOnlyCollection<ProfileGraph>(profileGraphs);
    }

    public ICollection<ProfileGraph> ProfileGraphs { get; private set; }

  }
}
