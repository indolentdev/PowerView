using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class Serie
  {
    public Serie(SerieName serieName, Unit unit, IEnumerable<double?> values)
    {
      if (serieName == null) throw new ArgumentNullException("serieName");

      SerieName = serieName;
      Unit = unit;
      Values = values.ToArray();
    }

    public SerieName SerieName { get; private set; }
    public Unit Unit { get; private set; }
    public double?[] Values { get; private set; }
  }
}

