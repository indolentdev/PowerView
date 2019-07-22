using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class Series
  {
    public Series(SeriesName serieName, Unit unit, IEnumerable<double?> values)
    {
      if (serieName == null) throw new ArgumentNullException("serieName");

      SeriesName = serieName;
      Unit = unit;
      Values = values.ToArray();
    }

    public SeriesName SeriesName { get; private set; }
    public Unit Unit { get; private set; }
    public double?[] Values { get; private set; }
  }
}
