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

    public void CalculateNext(NormalizedTimeRegisterValue timeRegisterValue)
    {
      NormalizedDurationRegisterValue generatedValue;
      if (generatedValues.Count == 0)
      {
        generatedValue = timeRegisterValue.SubtractAccommodateWrap(timeRegisterValue);
      }
      else
      {
        var minutend = timeRegisterValue;
        var substrahend = previous;
        if (!minutend.DeviceIdEquals(substrahend))
        {
          generatedValue = new NormalizedDurationRegisterValue(substrahend.TimeRegisterValue.Timestamp, minutend.TimeRegisterValue.Timestamp,
            substrahend.NormalizedTimestamp, minutend.NormalizedTimestamp, new UnitValue(0, minutend.TimeRegisterValue.UnitValue.Unit),
            substrahend.TimeRegisterValue.DeviceId, minutend.TimeRegisterValue.DeviceId, substrahend.TimeRegisterValue.DeviceId);
        }
        else
        {
          generatedValue = minutend.SubtractAccommodateWrap(substrahend);
        }
      }

      previous = timeRegisterValue;
      generatedValues.Add(generatedValue);
    }

    public IList<NormalizedDurationRegisterValue> GetGeneratedDurations()
    {
      return generatedValues.AsReadOnly();
    }
  }
}
