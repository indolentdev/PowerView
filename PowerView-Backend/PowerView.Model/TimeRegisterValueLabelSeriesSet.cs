using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class TimeRegisterValueLabelSeriesSet : LabelSeriesSet<TimeRegisterValue>
  {
    public TimeRegisterValueLabelSeriesSet(DateTime start, DateTime end, ICollection<TimeRegisterValueLabelSeries> labelSeries)
      : base(start, end, labelSeries.Select(x => (LabelSeries<TimeRegisterValue>)x).ToList())
    {
    }

    public LabelSeriesSet<NormalizedTimeRegisterValue> Normalize(Func<DateTime, DateTime> timeDivider)
    {
      if (timeDivider == null) throw new ArgumentNullException("timeDivider");

      var timeRegisterValueLabelSeries = this.Select(x => (TimeRegisterValueLabelSeries)x).ToList();

      return new LabelSeriesSet<NormalizedTimeRegisterValue>(Start, End, timeRegisterValueLabelSeries.Count, 
        timeRegisterValueLabelSeries.Select(x => x.Normalize(timeDivider)));
    }
  }

}
