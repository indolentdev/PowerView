using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class GaugeValueSet
  {
    public GaugeValueSet(GaugeSetName name, ICollection<GaugeValue> guageValues)
    {
      if (!Enum.IsDefined(typeof(GaugeSetName), name)) throw new ArgumentOutOfRangeException("name");
      if (guageValues == null) throw new ArgumentNullException("guageValues");
      if (guageValues.Any(gv => gv == null)) throw new ArgumentNullException("guageValues", "Items must not be null");

      Name = name;
      GuageValues = guageValues;
    }

    public GaugeSetName Name { get; private set; }
    public ICollection<GaugeValue> GuageValues { get; private set; }
  }
}
