using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace PowerView.Model
{
  public class LabelProfile : IEnumerable<ObisCode>
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Dictionary<ObisCode, ICollection<TimeRegisterValue>> obisCodeSets;

    public LabelProfile(string label, DateTime start, IDictionary<ObisCode, ICollection<TimeRegisterValue>> timeRegisterValuesByObisCode)
    {
      if ( string.IsNullOrEmpty(label) ) throw new ArgumentNullException("label");
      if ( start.Kind != DateTimeKind.Utc ) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if ( timeRegisterValuesByObisCode == null ) throw new ArgumentNullException("timeRegisterValuesByObisCode");
      Label = label;
      obisCodeSets = new Dictionary<ObisCode, ICollection<TimeRegisterValue>>(5);

      foreach (var obisCodeValueSet in timeRegisterValuesByObisCode)
      {
        PopulateObisCodeSets(label, start, obisCodeValueSet.Key, obisCodeValueSet.Value, timeRegisterValuesByObisCode.Keys);
      }
    }

    public string Label { get; private set; }

    public int GetValueCount()
    {
      return obisCodeSets.Sum(de => de.Value.Count);
    }

    public bool ContainsObisCode(ObisCode obisCode)
    {
      return obisCodeSets.ContainsKey(obisCode);
    }

    public ICollection<TimeRegisterValue> this[ObisCode obisCode] 
    {
      get 
      {
        return obisCodeSets.ContainsKey(obisCode) ? obisCodeSets[obisCode] : new TimeRegisterValue[0];
      }
    }

    #region IEnumerable implementation

    public IEnumerator<ObisCode> GetEnumerator()
    {
      return obisCodeSets.Keys.GetEnumerator();
    }

    #endregion

    #region IEnumerable implementation

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    public ICollection<ObisCode> GetAllObisCodes()
    {
      return obisCodeSets.Keys;
    }

    public UnitValue? GetLeakCharacteristic(ObisCode obisCode, DateTime start, DateTime end)
    {
      return GetLeakCharacteristic(obisCode, start, end, sv => sv.Timestamp.Hour);
    }

    public UnitValue? GetLeakCharacteristic(ObisCode obisCode, DateTime start, DateTime end, Func<TimeRegisterValue, int> timeGroupFunc, int minGroups = 5)
    {
      if (!obisCode.IsDelta) throw new ArgumentOutOfRangeException("obisCode", "Must be a delta obis code");
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if (end.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("end", "Must be UTC");
      if (timeGroupFunc == null) throw new ArgumentNullException("timeGroupFunc");

      var timeRegisterValues = this[obisCode].OrderBy(sv => sv.Timestamp).Where(sv => sv.Timestamp > start && sv.Timestamp < end);
      var hourly = new Dictionary<int, UnitValue>(6);
      try
      {
        foreach (var grouping in timeRegisterValues.GroupBy(timeGroupFunc, sv => sv))
        {
          hourly.Add(grouping.Key, grouping.Select(sv => sv.UnitValue).Sum());
        }
      }
      catch (DataMisalignedException e)
      {
        log.Info("Unable to check of leak characteristic. Data error", e);
        return null;
      }

      if (hourly.Count < minGroups)
      {
        return null;
      }

      var hourlyGreaterThanZero = hourly.Where(de => de.Value.Value > 0).ToArray();
      var hasLeakCharacteristic = hourlyGreaterThanZero.Length == hourly.Count;

      return hasLeakCharacteristic ? hourly.Values.Sum() : new UnitValue(0, hourly.First().Value.Unit);
    }

    private void PopulateObisCodeSets(string label, DateTime start, ObisCode obisCode, ICollection<TimeRegisterValue> timeRegisterValues, ICollection<ObisCode> source)
    {
      var orderedTimeRegisterValues = timeRegisterValues.OrderBy(sv => sv.Timestamp).ToArray();

      try
      {
        var units = orderedTimeRegisterValues.Select(x => x.UnitValue.Unit).Distinct().ToArray();
        if (units.Length > 1)
        {
          throw new DataMisalignedException(string.Format("Failed to identify single unit. Possible units:{0}",
            string.Join(",", units.Select(u => u.ToString()))));
        }

        obisCodeSets.Add(obisCode, orderedTimeRegisterValues.Where(sv => sv.Timestamp >= start).ToArray());

        if ( obisCode.IsCumulative )
        {
          GenerateValues(obisCode.ToInterim(), GetPeriodValues(start, orderedTimeRegisterValues));
          GenerateValues(obisCode.ToDelta(), GetDeltaValues(orderedTimeRegisterValues).Where(sv => sv.Timestamp >= start));

          var actualObisCode = GetActualObisCode(obisCode);
          if (actualObisCode != null && !source.Contains(actualObisCode.Value))
          {
            GenerateValues(actualObisCode.Value, GetActualValues(orderedTimeRegisterValues).Where(sv => sv.Timestamp >= start));
          }
        }
      }
      catch (DataMisalignedException e)
      {
        var msg = string.Format("Failed to map values. If the meter was replaced the data must be cleaned up. Label:{0}, ObisCode:{1}, Values:{2}", 
          label, obisCode, string.Join(",", timeRegisterValues.Select(sv => sv.ToString())));
        throw new DataMisalignedException(msg, e);
      }
    }

    private void GenerateValues(ObisCode obisCode, IEnumerable<TimeRegisterValue> values)
    {
      var valuesArray = values.ToArray();
      if (valuesArray.Length == 0)
      {
        return;
      }

      obisCodeSets.Add(obisCode, valuesArray);
    }

    private static IEnumerable<TimeRegisterValue> GetPeriodValues(DateTime start, TimeRegisterValue[] orderedTimeRegisterValues)
    {
      var snReferenceValues = new List<TimeRegisterValue>(2);
      var snTransitionValues = new Dictionary<string, TimeRegisterValue>(2);

      foreach (var timeRegisterValue in orderedTimeRegisterValues)
      {
        if (timeRegisterValue.Timestamp < start)
        {
          if (snReferenceValues.Count == 1)
          {
            snReferenceValues.Remove(snReferenceValues.Last());
          }
          snReferenceValues.Add(timeRegisterValue);
          continue;
        }

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

        yield return Sum(snTransitionValues.Values);
      }
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

        addend = new TimeRegisterValue(addendSerialNumber, timeRegisterValue.Timestamp, timeRegisterValue.UnitValue.Value+addend.Value.UnitValue.Value, timeRegisterValue.UnitValue.Unit);
      }
      return addend.Value;
    }

    private static IEnumerable<TimeRegisterValue> GetDeltaValues(TimeRegisterValue[] orderedTimeRegisterValues)
    {
      for (var ix = 0; ix < orderedTimeRegisterValues.Length; ix++)
      {
        if (ix == 0)
        {
          yield return orderedTimeRegisterValues[0].SubtractValue(orderedTimeRegisterValues[0]);
        }
        else
        {
          var minutend = orderedTimeRegisterValues[ix];
          var substrahend = orderedTimeRegisterValues[ix-1];
          if (minutend.SerialNumber != substrahend.SerialNumber)
          {
            yield return new TimeRegisterValue(minutend.SerialNumber, minutend.Timestamp, 0, minutend.UnitValue.Unit);
          }
          else
          {
            yield return minutend.SubtractValue(substrahend);
          }
        }
      }
    }

    private static ObisCode? GetActualObisCode(ObisCode obisCode)
    {
      if (obisCode == ObisCode.ActiveEnergyA14)
      {
        return ObisCode.ActualPowerP14;
      }
      else if (obisCode == ObisCode.ActiveEnergyA23)
      {
        return ObisCode.ActualPowerP23;
      }
      else if (obisCode == ObisCode.ColdWaterVolume1)
      {
        return ObisCode.ColdWaterFlow1;
      }
      else if (obisCode == ObisCode.HeatEnergyEnergy1)
      {
        return ObisCode.HeatEnergyPower1;
      }
      else if (obisCode == ObisCode.HeatEnergyVolume1)
      {
        return ObisCode.HeatEnergyFlow1;
      }

      return null;
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

    private static IEnumerable<TimeRegisterValue> GetActualValues(TimeRegisterValue[] orderedTimeRegisterValues)
    {
      for (var ix = 0; ix < orderedTimeRegisterValues.Length; ix++)
      {
        if (ix == 0)
        {
          var timeRegisterValue = orderedTimeRegisterValues[0];
          yield return new TimeRegisterValue(timeRegisterValue.SerialNumber, DateTime.MinValue.ToUniversalTime(), 0, GetActualUnit(timeRegisterValue.UnitValue.Unit));
        }
        else
        {
          var minutend = orderedTimeRegisterValues[ix];
          var substrahend = orderedTimeRegisterValues[ix-1];
          if (minutend.SerialNumber != substrahend.SerialNumber)
          {
            yield return new TimeRegisterValue(minutend.SerialNumber, DateTime.MinValue.ToUniversalTime(), 0, GetActualUnit(minutend.UnitValue.Unit));
          }
          else
          {
            var duration = minutend.Timestamp - substrahend.Timestamp;

            if (duration > TimeSpan.FromMinutes(6) || duration < TimeSpan.FromMinutes(1.6))
            {
              yield return new TimeRegisterValue(minutend.SerialNumber, DateTime.MinValue.ToUniversalTime(), 0, GetActualUnit(minutend.UnitValue.Unit));
            }
            else
            {
              var actualUnit = GetActualUnit(minutend.UnitValue.Unit);
              var delta = minutend.SubtractValue(substrahend).UnitValue.Value;
              var actualValue = delta / duration.TotalHours;
              yield return new TimeRegisterValue(minutend.SerialNumber, minutend.Timestamp, actualValue, actualUnit);
            }
          }
        }
      }
    }

  }
}
