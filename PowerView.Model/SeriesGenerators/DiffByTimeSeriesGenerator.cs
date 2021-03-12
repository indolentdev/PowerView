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

      // TODO: Move substraction else where?
      var differenceValue = minutend.UnitValue.Value - substrahend.UnitValue.Value;
      if (differenceValue < 0)
      {
        differenceValue = 0;
      }
      var difference = new UnitValue(differenceValue, minutend.UnitValue.Unit);
      var start = new DateTime(Math.Min(minutend.Start.Ticks, substrahend.Start.Ticks), DateTimeKind.Utc);
      var end = new DateTime(Math.Max(minutend.End.Ticks, substrahend.End.Ticks), DateTimeKind.Utc);
      var generatedValue = new NormalizedDurationRegisterValue(start, end, minutend.NormalizedStart, minutend.NormalizedEnd,
        difference, DeviceId.DistinctDeviceIds(minutend.DeviceIds.Concat(substrahend.DeviceIds)));

      generatedValues.Add(generatedValue);
    }

    public IList<NormalizedDurationRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
