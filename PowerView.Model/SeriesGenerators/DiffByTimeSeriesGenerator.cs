using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.SeriesGenerators
{
  public class DiffByTimeSeriesGenerator : IMultiInputSeriesGenerator
  {
    private readonly ObisCode minuendObisCode;
    private readonly ObisCode substrahendObisCode;
    private readonly List<NormalizedDurationRegisterValue> generatedValues;

    public DiffByTimeSeriesGenerator(ObisCode minuendObisCode, ObisCode substrahendObisCode)
    {
      this.minuendObisCode = minuendObisCode;
      this.substrahendObisCode = substrahendObisCode;
      generatedValues = new List<NormalizedDurationRegisterValue>();
    }

    public bool IsSatisfiedBy(IDictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>> values)
    {
      return values.ContainsKey(minuendObisCode) && values.ContainsKey(substrahendObisCode);
    }

    public void CalculateNext(IDictionary<ObisCode, NormalizedDurationRegisterValue> obisCodeRegisterValues)
    {
      NormalizedDurationRegisterValue minutend;
      NormalizedDurationRegisterValue substrahend;
      if (!obisCodeRegisterValues.TryGetValue(minuendObisCode, out minutend) || !obisCodeRegisterValues.TryGetValue(substrahendObisCode, out substrahend))
      {
        return;
      }

      if (minutend.NormalizedStart != substrahend.NormalizedStart || minutend.NormalizedEnd != substrahend.NormalizedEnd)
      {
        return;
      }

      if (minutend.UnitValue.Unit != substrahend.UnitValue.Unit)
      {
        return;
      }

      var value = minutend.SubtractNotNegative(substrahend);
      generatedValues.Add(value);
    }

    public IList<NormalizedDurationRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
