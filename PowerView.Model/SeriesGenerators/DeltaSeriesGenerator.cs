using System.Linq;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public class DeltaSeriesGenerator : ISingleInputSeriesGenerator
  {
    private readonly List<NormalizedDurationRegisterValue> generatedValues;
    private NormalizedTimeRegisterValue previous;

    public DeltaSeriesGenerator()
    {
      generatedValues = new List<NormalizedDurationRegisterValue>(300);
    }

    public void CalculateNext(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
    {
      NormalizedDurationRegisterValue generatedValue;
      if (generatedValues.Count == 0)
      {
        generatedValue = normalizedTimeRegisterValue.SubtractAccommodateWrap(normalizedTimeRegisterValue);
      }
      else
      {
        var minutend = normalizedTimeRegisterValue;
        var substrahend = previous;
        if (!minutend.DeviceIdEquals(substrahend))
        {
          generatedValue = new NormalizedDurationRegisterValue(substrahend.TimeRegisterValue.Timestamp, minutend.TimeRegisterValue.Timestamp,
            substrahend.NormalizedTimestamp, minutend.NormalizedTimestamp, new UnitValue(0, minutend.TimeRegisterValue.UnitValue.Unit),
            substrahend.TimeRegisterValue.DeviceId, minutend.TimeRegisterValue.DeviceId);
        }
        else
        {
          generatedValue = minutend.SubtractAccommodateWrap(substrahend);
        }
      }

      previous = normalizedTimeRegisterValue;
      generatedValues.Add(generatedValue);
    }

    public IList<NormalizedDurationRegisterValue> GetGeneratedDurations()
    {
      return generatedValues.AsReadOnly();
    }

    public IList<NormalizedTimeRegisterValue> GetGenerated()
    {
      return generatedValues.Select(x => new NormalizedTimeRegisterValue(
        new TimeRegisterValue(x.DeviceIds.Last(), x.End, x.UnitValue), x.NormalizedEnd)).ToList().AsReadOnly();
    }

  }
}
