using System;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public class AverageActualSeriesGenerator : ISeriesGenerator
  {
    private readonly List<NormalizedTimeRegisterValue> generatedValues;
    private NormalizedTimeRegisterValue previous;

    public AverageActualSeriesGenerator()
    {
      generatedValues = new List<NormalizedTimeRegisterValue>(300);
    }

    public void CalculateNext(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
    {
      NormalizedTimeRegisterValue generatedValue;
      if (generatedValues.Count == 0)
      {
        generatedValue = new NormalizedTimeRegisterValue(
          new TimeRegisterValue(normalizedTimeRegisterValue.TimeRegisterValue.DeviceId, normalizedTimeRegisterValue.TimeRegisterValue.Timestamp, 0, GetActualUnit(normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Unit)),
          normalizedTimeRegisterValue.NormalizedTimestamp);
      }
      else
      {
        var minutend = normalizedTimeRegisterValue;
        var substrahend = previous;
        if (!minutend.DeviceIdEquals(substrahend))
        {
          generatedValue = new NormalizedTimeRegisterValue(
            new TimeRegisterValue(minutend.TimeRegisterValue.DeviceId, minutend.TimeRegisterValue.Timestamp, 0, GetActualUnit(normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Unit)),
            minutend.NormalizedTimestamp);
        }
        else
        {
          var duration = minutend.TimeRegisterValue.Timestamp - substrahend.TimeRegisterValue.Timestamp;
          var unit = GetActualUnit(minutend.TimeRegisterValue.UnitValue.Unit);
          var delta = minutend.TimeRegisterValue.SubtractValue(substrahend.TimeRegisterValue).UnitValue.Value;
          var averageActualValue = delta / duration.TotalHours;
          generatedValue = new NormalizedTimeRegisterValue(
            new TimeRegisterValue(minutend.TimeRegisterValue.DeviceId, minutend.TimeRegisterValue.Timestamp, averageActualValue, unit),
            minutend.NormalizedTimestamp);
        }
      }

      previous = normalizedTimeRegisterValue;
      generatedValues.Add(generatedValue);
    }

    private static Unit GetActualUnit(Unit unit)
    {
      if (unit == Unit.WattHour)
      {
        return Unit.Watt;
      }
      if (unit == Unit.CubicMetre)
      {
        return Unit.CubicMetrePrHour;
      }

      return (Unit)250;
    }

    public IList<NormalizedTimeRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
