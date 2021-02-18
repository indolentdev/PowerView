using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class TimeRegisterValueLabelSeries : LabelSeries<TimeRegisterValue>
  {
    public TimeRegisterValueLabelSeries(string label, IDictionary<ObisCode, IEnumerable<TimeRegisterValue>> timeRegisterValuesByObisCode)
      : base(label, timeRegisterValuesByObisCode)
    {
    }

    public LabelSeries<NormalizedTimeRegisterValue> Normalize(Func<DateTime, DateTime> timeDivider)
    {
      if (timeDivider == null) throw new ArgumentNullException("timeDivider");

      var normalized = new Dictionary<ObisCode, IEnumerable<NormalizedTimeRegisterValue>>();
      foreach (var obisCode in this)
      {
        var values = this[obisCode];
        // The GroupBy and Select(x.First()) relies on the ordering provided by GetOrderedReadOnlyList above.
        // Confer the MSDN remark for GroupBy: 
        // https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?redirectedfrom=MSDN&view=netframework-4.8#System_Linq_Enumerable_GroupBy__3_System_Collections_Generic_IEnumerable___0__System_Func___0___1__System_Func___0___2__
        var normalizedValues = values.Select(x => x.Normalize(timeDivider)).GroupBy(x => x.NormalizedTimestamp).Select(x => x.First());
        normalized.Add(obisCode, normalizedValues);
      }
      return new LabelSeries<NormalizedTimeRegisterValue>(Label, normalized);
    }

  }
}
