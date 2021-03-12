using System;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public class DiffByTimeSeriesGeneratorOld
  {
    private readonly ObisCode minuendObisCode;
    private readonly ObisCode substrahendObisCode;
    private readonly List<NormalizedTimeRegisterValue> generatedValues;

    public DiffByTimeSeriesGeneratorOld(ObisCode minuendObisCode, ObisCode substrahendObisCode)
    {
      this.minuendObisCode = minuendObisCode;
      this.substrahendObisCode = substrahendObisCode;
      generatedValues = new List<NormalizedTimeRegisterValue>();
    }

    public bool IsSatisfiedBy(IDictionary<ObisCode, IList<NormalizedTimeRegisterValue>> values)
    {
      return values.ContainsKey(minuendObisCode) && values.ContainsKey(substrahendObisCode);
    }

    public void CalculateNext(DateTime normalizedTimestamp, IDictionary<ObisCode, TimeRegisterValue> obisCodeRegisterValues)
    {
      TimeRegisterValue minutend;
      TimeRegisterValue substrahend;
      if (!obisCodeRegisterValues.TryGetValue(minuendObisCode, out minutend) || !obisCodeRegisterValues.TryGetValue(substrahendObisCode, out substrahend))
      {
        return;
      }

      if (!minutend.DeviceIdEquals(substrahend) || minutend.UnitValue.Unit != substrahend.UnitValue.Unit)
      {
        return;
      }

      var differenceValue = minutend.UnitValue.Value - substrahend.UnitValue.Value;
      if (differenceValue < 0)
      {
        differenceValue = 0;
      }
      var difference = new UnitValue(differenceValue, minutend.UnitValue.Unit);
      var timestamp = substrahend.Timestamp + TimeSpan.FromTicks((minutend.Timestamp - substrahend.Timestamp).Ticks / 2);
      var diffTimeRegisterValue = new TimeRegisterValue(minutend.DeviceId, timestamp, difference);
      var generatedValue = new NormalizedTimeRegisterValue(diffTimeRegisterValue, normalizedTimestamp);

      generatedValues.Add(generatedValue);
    }

    public IList<NormalizedTimeRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
