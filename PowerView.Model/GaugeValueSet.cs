using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class GaugeValueSet
  {
    public GaugeValueSet(GaugeSetName name, ICollection<GaugeValue> gaugeValues)
    {
      if (!Enum.IsDefined(typeof(GaugeSetName), name)) throw new ArgumentOutOfRangeException("name");
      if (gaugeValues == null) throw new ArgumentNullException("gaugeValues");
      if (gaugeValues.Any(gv => gv == null)) throw new ArgumentNullException("gaugeValues", "Items must not be null");

      Name = name;
      GaugeValues = gaugeValues;
    }

    public GaugeSetName Name { get; private set; }
    public ICollection<GaugeValue> GaugeValues { get; private set; }
  }
}
