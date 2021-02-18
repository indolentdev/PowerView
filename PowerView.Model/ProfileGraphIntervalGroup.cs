using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerView.Model
{
  public class ProfileGraphIntervalGroup : IntervalGroup
  {
    private readonly Func<DateTime, DateTime> timeDivider;
    private readonly Func<DateTime, DateTime> getNext;

    public ProfileGraphIntervalGroup(TimeZoneInfo timeZoneinfo, DateTime start, string interval, IList<ProfileGraph> profileGraphs, LabelSeriesSet<TimeRegisterValue> labelSeriesSet)
      : base(timeZoneinfo, start, interval, labelSeriesSet)
    {
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");

      ProfileGraphs = new ReadOnlyCollection<ProfileGraph>(profileGraphs);
    }

    public ICollection<ProfileGraph> ProfileGraphs { get; private set; }

  }
}
