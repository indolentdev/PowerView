using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class LabelSeriesSet<T> : IEnumerable<LabelSeries<T>> where T : class, IRegisterValue
  {
    private readonly List<LabelSeries<T>> labelSeries;

    public LabelSeriesSet(DateTime start, DateTime end, ICollection<LabelSeries<T>> labelSeries)
      : this(start, end, labelSeries != null ? labelSeries.Count : 0, labelSeries)
    {
    }

    internal LabelSeriesSet(DateTime start, DateTime end, int estimatedLabelSeriesCount, IEnumerable<LabelSeries<T>> labelSeries)
    {
      ArgCheck.ThrowIfNotUtc(start);
      ArgCheck.ThrowIfNotUtc(end);
      ArgumentNullException.ThrowIfNull(labelSeries);

      Start = start;
      End = end;

      this.labelSeries = new List<LabelSeries<T>>(estimatedLabelSeriesCount);
      this.labelSeries.AddRange(labelSeries);
    }

    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    internal void Add(LabelSeries<T> ls)
    {
      labelSeries.Add(ls);
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
