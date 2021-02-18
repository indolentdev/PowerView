using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerView.Model
{
  public class ProfileGraphIntervalGroup : IntervalGroup
  {
    public ProfileGraphIntervalGroup(TimeZoneInfo timeZoneinfo, DateTime start, string interval, IList<ProfileGraph> profileGraphs, TimeRegisterValueLabelSeriesSet labelSeriesSet)
      : base(timeZoneinfo, start, interval, labelSeriesSet)
    {
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");

      ProfileGraphs = new ReadOnlyCollection<ProfileGraph>(profileGraphs);
    }

    public ICollection<ProfileGraph> ProfileGraphs { get; private set; }

  }
}
