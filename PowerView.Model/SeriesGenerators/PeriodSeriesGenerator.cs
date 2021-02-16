using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.SeriesGenerators
{
  public class PeriodSeriesGenerator : ISingleInputSeriesGenerator
  {
    private readonly List<NormalizedTimeRegisterValue> snReferenceValues;
    private readonly Dictionary<string, NormalizedTimeRegisterValue> snTransitionValues;
    private readonly List<NormalizedTimeRegisterValue> generatedValues;

    public PeriodSeriesGenerator()
    {
      snReferenceValues = new List<NormalizedTimeRegisterValue>(2);
      snTransitionValues = new Dictionary<string, NormalizedTimeRegisterValue>(2);
      generatedValues = new List<NormalizedTimeRegisterValue>(300);
    }

    public void CalculateNext(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
    {
      if (snReferenceValues.Count == 0)
      {
        snReferenceValues.Add(normalizedTimeRegisterValue);
      }

      var reference = snReferenceValues[snReferenceValues.Count - 1];
      if (!reference.DeviceIdEquals(normalizedTimeRegisterValue))
      {
        snReferenceValues.Add(normalizedTimeRegisterValue);
        reference = normalizedTimeRegisterValue;
      }

      var value = normalizedTimeRegisterValue.TimeRegisterValue.SubtractValue(reference.TimeRegisterValue);
      var normalizedValue = new NormalizedTimeRegisterValue(value, normalizedTimeRegisterValue.NormalizedTimestamp);
      snTransitionValues[GetTransitionKey(reference)] = normalizedValue;

      var generatedValue = Sum(snTransitionValues.Values);
      generatedValues.Add(generatedValue);
    }

    public IList<NormalizedTimeRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }

    private static string GetTransitionKey(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
    {
      return string.Format("SN:{0}-RefTime:{1}", normalizedTimeRegisterValue.TimeRegisterValue.DeviceId, normalizedTimeRegisterValue.TimeRegisterValue.Timestamp.ToString("o"));
    }

    private static NormalizedTimeRegisterValue Sum(ICollection<NormalizedTimeRegisterValue> normalizedTimeRegisterValues)
    {
      NormalizedTimeRegisterValue addend = null;
      foreach (var normalizedTimeRegisterValue in normalizedTimeRegisterValues.OrderBy(x => x.OrderProperty))
      {
        if (addend == null)
        {
          addend = normalizedTimeRegisterValue;
          continue;
        }

        if (normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Unit != addend.TimeRegisterValue.UnitValue.Unit)
        {
          throw new DataMisalignedException("A calculation of a value sum was not possible. Units of values differ. Units:" +
            normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Unit + ", " + addend.TimeRegisterValue.UnitValue.Unit);
        }

        var addendDeviceId = addend.DeviceIdEquals(normalizedTimeRegisterValue) ? normalizedTimeRegisterValue.TimeRegisterValue.DeviceId : TimeRegisterValue.DummyDeviceId;
        addend = new NormalizedTimeRegisterValue(
          new TimeRegisterValue(addendDeviceId, 
            normalizedTimeRegisterValue.TimeRegisterValue.Timestamp, 
            normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Value + addend.TimeRegisterValue.UnitValue.Value, 
            normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Unit),
          normalizedTimeRegisterValue.NormalizedTimestamp);
      }
      return addend;
    }

  }
}
