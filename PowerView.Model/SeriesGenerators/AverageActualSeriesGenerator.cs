using System;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public class AverageActualSeriesGenerator : ISeriesGenerator
  {
    private readonly List<TimeRegisterValue> generatedValues;
    private TimeRegisterValue previous;

    public AverageActualSeriesGenerator()
    {
      generatedValues = new List<TimeRegisterValue>(300);
    }

    public void CalculateNext(TimeRegisterValue timeRegisterValue)
    {
      TimeRegisterValue generatedValue;
      if (generatedValues.Count == 0)
      {
        generatedValue = new TimeRegisterValue(timeRegisterValue.SerialNumber, timeRegisterValue.Timestamp, 0, GetActualUnit(timeRegisterValue.UnitValue.Unit));
      }
      else
      {
        var minutend = timeRegisterValue;
        var substrahend = previous;
        if (!string.Equals(minutend.SerialNumber, substrahend.SerialNumber, StringComparison.OrdinalIgnoreCase))
        {
          generatedValue = new TimeRegisterValue(minutend.SerialNumber, minutend.Timestamp, 0, GetActualUnit(timeRegisterValue.UnitValue.Unit));
        }
        else
        {
          var duration = minutend.Timestamp - substrahend.Timestamp;
          var unit = GetActualUnit(minutend.UnitValue.Unit);
          var delta = minutend.SubtractValue(substrahend).UnitValue.Value;
          var averageActualValue = delta / duration.TotalHours;
          generatedValue = new TimeRegisterValue(minutend.SerialNumber, minutend.Timestamp, averageActualValue, unit);
        }
      }

      previous = timeRegisterValue;
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

    public IList<TimeRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
