using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.SeriesGenerators
{
  public class PeriodSeriesGenerator : ISeriesGenerator
  {
    private readonly List<TimeRegisterValue> snReferenceValues;
    private readonly Dictionary<string, TimeRegisterValue> snTransitionValues;
    private readonly List<TimeRegisterValue> generatedValues;

    public PeriodSeriesGenerator()
    {
      snReferenceValues = new List<TimeRegisterValue>(2);
      snTransitionValues = new Dictionary<string, TimeRegisterValue>(2);
      generatedValues = new List<TimeRegisterValue>(300);
    }

    public void CalculateNext(TimeRegisterValue timeRegisterValue)
    {
      if (snReferenceValues.Count == 0)
      {
        snReferenceValues.Add(timeRegisterValue);
      }

      var reference = snReferenceValues[snReferenceValues.Count - 1];
      if (!string.Equals(reference.SerialNumber, timeRegisterValue.SerialNumber, StringComparison.OrdinalIgnoreCase))
      {
        snReferenceValues.Add(timeRegisterValue);
        reference = timeRegisterValue;
      }

      var value = timeRegisterValue.SubtractValue(reference);
      snTransitionValues[GetTransitionKey(reference)] = value;

      var generatedValue = Sum(snTransitionValues.Values);
      generatedValues.Add(generatedValue);
    }

    public IList<TimeRegisterValue> GetGenerated()
    {
      return generatedValues.AsReadOnly();
    }

    private static string GetTransitionKey(TimeRegisterValue timeRegisterValue)
    {
      return string.Format("SN:{0}-RefTime:{1}", timeRegisterValue.SerialNumber, timeRegisterValue.Timestamp.ToString("o"));
    }

    private static TimeRegisterValue Sum(ICollection<TimeRegisterValue> timeRegisterValues)
    {
      TimeRegisterValue? addend = null;
      foreach (var timeRegisterValue in timeRegisterValues.OrderBy(x => x.Timestamp))
      {
        if (addend == null)
        {
          addend = timeRegisterValue;
          continue;
        }

        if (timeRegisterValue.UnitValue.Unit != addend.Value.UnitValue.Unit)
        {
          throw new DataMisalignedException("A calculation of a value sum was not possible. Units of values differ. Units:" +
            timeRegisterValue.UnitValue.Unit + ", " + addend.Value.UnitValue.Unit);
        }

        var addendSerialNumber = string.Equals(addend.Value.SerialNumber, timeRegisterValue.SerialNumber, StringComparison.InvariantCultureIgnoreCase) ?
          timeRegisterValue.SerialNumber : TimeRegisterValue.DummySerialNumber;

        addend = new TimeRegisterValue(addendSerialNumber, timeRegisterValue.Timestamp, timeRegisterValue.UnitValue.Value + addend.Value.UnitValue.Value, timeRegisterValue.UnitValue.Unit);
      }
      return addend.Value;
    }

  }
}
