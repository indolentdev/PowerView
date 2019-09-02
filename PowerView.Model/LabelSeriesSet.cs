using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class LabelSeriesSet<T> : IEnumerable<LabelSeries<T>> where T : struct, ISeries
  {
    private readonly List<LabelSeries<T>> labelSeries;

    public LabelSeriesSet(DateTime start, DateTime end, ICollection<LabelSeries<T>> labelSeries)
      : this(start, end, labelSeries != null ? labelSeries.Count : 0, labelSeries)
    {
    }

    internal LabelSeriesSet(DateTime start, DateTime end, int estimatedLabelSeriesCount, IEnumerable<LabelSeries<T>> labelSeries)
    {
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if (end.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("end", "Must be UTC");
      if (labelSeries == null) throw new ArgumentNullException("labelSeries");

      Start = start;
      End = end;

      this.labelSeries = new List<LabelSeries<T>>(estimatedLabelSeriesCount);
      this.labelSeries.AddRange(labelSeries);
    }

    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    public LabelSeriesSet<NormalizedTimeRegisterValue> Normalize(Func<DateTime, DateTime> timeDivider)
    {
      if (timeDivider == null) throw new ArgumentNullException("timeDivider");

      return new LabelSeriesSet<NormalizedTimeRegisterValue>(Start, End, labelSeries.Count, labelSeries.Select(x => x.Normalize(timeDivider)));
    }

    internal void Add(IList<LabelSeries<T>> list)
    {
      labelSeries.AddRange(list);
    }

    #region IEnumerable implementation
    public IEnumerator<LabelSeries<T>> GetEnumerator()
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
  }

}
