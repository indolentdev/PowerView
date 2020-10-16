using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace PowerView.Model
{
  public class LeakCharacteristicChecker
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public UnitValue? GetLeakCharacteristic(LabelSeries<NormalizedTimeRegisterValue> labelSeries, ObisCode obisCode, DateTime start, DateTime end)
    {
      return GetLeakCharacteristic(labelSeries, obisCode, start, end, sv => sv.TimeRegisterValue.Timestamp.Hour);
    }

    public UnitValue? GetLeakCharacteristic(LabelSeries<NormalizedTimeRegisterValue> labelSeries, ObisCode obisCode, DateTime start, DateTime end, Func<NormalizedTimeRegisterValue, int> timeGroupFunc, int minGroups = 5)
    {
      if (labelSeries == null) throw new ArgumentNullException("labelSeries");
      if (!obisCode.IsDelta) throw new ArgumentOutOfRangeException("obisCode", "Must be a delta obis code");
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if (end.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("end", "Must be UTC");
      if (timeGroupFunc == null) throw new ArgumentNullException("timeGroupFunc");

      var obisCodeTimeRegisterValues = labelSeries[obisCode];
      var timeRegisterValues = obisCodeTimeRegisterValues.Where(sv => sv.TimeRegisterValue.Timestamp > start && sv.TimeRegisterValue.Timestamp < end).ToList();
      var hourly = new Dictionary<int, UnitValue>(6);
      try
      {
        foreach (var grouping in timeRegisterValues.GroupBy(timeGroupFunc, sv => sv))
        {
          hourly.Add(grouping.Key, grouping.Select(sv => sv.TimeRegisterValue.UnitValue).Sum());
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

  }
}
