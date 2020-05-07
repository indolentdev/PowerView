using System;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public class DeltaSeriesGenerator : ISeriesGenerator
  {
    private readonly List<NormalizedTimeRegisterValue> generatedValues;
    private NormalizedTimeRegisterValue previous;

    public DeltaSeriesGenerator()
    {
      generatedValues = new List<NormalizedTimeRegisterValue>(300);
    }

    public void CalculateNext(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
    {
      NormalizedTimeRegisterValue generatedValue;
      if (generatedValues.Count == 0)
      {
        generatedValue = new NormalizedTimeRegisterValue(
          normalizedTimeRegisterValue.TimeRegisterValue.SubtractValue(normalizedTimeRegisterValue.TimeRegisterValue),
          normalizedTimeRegisterValue.NormalizedTimestamp);
      }
      else
      {
        var minutend = normalizedTimeRegisterValue;
        var substrahend = previous;
        if (!minutend.DeviceIdEquals(substrahend))
        {
          generatedValue = new NormalizedTimeRegisterValue(
            new TimeRegisterValue(minutend.TimeRegisterValue.DeviceId, minutend.TimeRegisterValue.Timestamp, 0, minutend.TimeRegisterValue.UnitValue.Unit),
            minutend.NormalizedTimestamp);
        }
        else
        {
          generatedValue = new NormalizedTimeRegisterValue(minutend.TimeRegisterValue.SubtractValue(substrahend.TimeRegisterValue), minutend.NormalizedTimestamp);
        }
      }

      previous = normalizedTimeRegisterValue;
      generatedValues.Add(generatedValue);
    }

    public IList<NormalizedTimeRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
