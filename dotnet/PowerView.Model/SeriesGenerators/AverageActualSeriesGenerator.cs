using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.SeriesGenerators
{
  public class AverageActualSeriesGenerator : ISingleInputSeriesGenerator
  {
    private readonly List<NormalizedDurationRegisterValue> generatedValues;
    private NormalizedTimeRegisterValue previous;

    public AverageActualSeriesGenerator()
    {
      generatedValues = new List<NormalizedDurationRegisterValue>(300);
    }

    public void CalculateNext(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
    {
      var actualUnit = GetActualUnit(normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Unit);

      NormalizedDurationRegisterValue generatedValue;
      if (generatedValues.Count == 0)
      {
        generatedValue = new NormalizedDurationRegisterValue(
          normalizedTimeRegisterValue.TimeRegisterValue.Timestamp, normalizedTimeRegisterValue.TimeRegisterValue.Timestamp,
          normalizedTimeRegisterValue.NormalizedTimestamp, normalizedTimeRegisterValue.NormalizedTimestamp,
          new UnitValue(0, actualUnit), normalizedTimeRegisterValue.TimeRegisterValue.DeviceId);
      }
      else
      {
        var minutend = normalizedTimeRegisterValue;
        var substrahend = previous;
        if (!minutend.DeviceIdEquals(substrahend))
        {
          generatedValue = new NormalizedDurationRegisterValue(
            substrahend.TimeRegisterValue.Timestamp, minutend.TimeRegisterValue.Timestamp, substrahend.NormalizedTimestamp, minutend.NormalizedTimestamp,
            new UnitValue(0, actualUnit), substrahend.TimeRegisterValue.DeviceId, minutend.TimeRegisterValue.DeviceId);
        }
        else if (substrahend.TimeRegisterValue.UnitValue.Unit != minutend.TimeRegisterValue.UnitValue.Unit)
        {
          generatedValue = new NormalizedDurationRegisterValue(
            substrahend.TimeRegisterValue.Timestamp, minutend.TimeRegisterValue.Timestamp, substrahend.NormalizedTimestamp, minutend.NormalizedTimestamp,
            new UnitValue(0, actualUnit), minutend.TimeRegisterValue.DeviceId);
        }
        else
        {
          var duration = minutend.TimeRegisterValue.Timestamp - substrahend.TimeRegisterValue.Timestamp;
          var delta = minutend.SubtractAccommodateWrap(substrahend).UnitValue.Value;
          var averageActualValue = delta / duration.TotalHours; // assume average by hour..
          generatedValue = new NormalizedDurationRegisterValue(
            substrahend.TimeRegisterValue.Timestamp, minutend.TimeRegisterValue.Timestamp, substrahend.NormalizedTimestamp, minutend.NormalizedTimestamp,
            new UnitValue(averageActualValue, actualUnit), minutend.TimeRegisterValue.DeviceId);
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

    public IList<NormalizedDurationRegisterValue> GetGeneratedDurations()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
