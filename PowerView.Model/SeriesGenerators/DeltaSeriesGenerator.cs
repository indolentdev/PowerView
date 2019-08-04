using System;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public class DeltaSeriesGenerator : ISeriesGenerator
  {
    private readonly List<TimeRegisterValue> generatedValues;
    private TimeRegisterValue previous;

    public DeltaSeriesGenerator()
    {
      generatedValues = new List<TimeRegisterValue>(300);
    }

    public void CalculateNext(TimeRegisterValue timeRegisterValue)
    {
      TimeRegisterValue generatedValue;
      if (generatedValues.Count == 0)
      {
        generatedValue = timeRegisterValue.SubtractValue(timeRegisterValue);
      }
      else
      {
        var minutend = timeRegisterValue;
        var substrahend = previous;
        if (!string.Equals(minutend.SerialNumber, substrahend.SerialNumber, StringComparison.OrdinalIgnoreCase))
        {
          generatedValue = new TimeRegisterValue(minutend.SerialNumber, minutend.Timestamp, 0, minutend.UnitValue.Unit);
        }
        else
        {
          generatedValue = minutend.SubtractValue(substrahend);
        }
      }

      previous = timeRegisterValue;
      generatedValues.Add(generatedValue);
    }

    public IList<TimeRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
