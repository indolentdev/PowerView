using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace PowerView.Model
{
  public class LeakCharacteristicChecker : ILeakCharacteristicChecker
  {
    private readonly ILogger<LeakCharacteristicChecker> logger;

    public LeakCharacteristicChecker(ILogger<LeakCharacteristicChecker> logger)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public UnitValue? GetLeakCharacteristic(LabelSeries<NormalizedDurationRegisterValue> labelSeries, ObisCode obisCode, DateTime start, DateTime end)
    {
      return GetLeakCharacteristic(labelSeries, obisCode, start, end, x => x.End.Hour);
    }

    public UnitValue? GetLeakCharacteristic(LabelSeries<NormalizedDurationRegisterValue> labelSeries, ObisCode obisCode, DateTime start, DateTime end, Func<NormalizedDurationRegisterValue, int> timeGroupFunc, int minGroups = 5)
    {
      ArgumentNullException.ThrowIfNull(labelSeries);
      if (!obisCode.IsDelta) throw new ArgumentOutOfRangeException(nameof(obisCode), "Must be a delta obis code");
      ArgCheck.ThrowIfNotUtc(start);
      ArgCheck.ThrowIfNotUtc(end);
      ArgumentNullException.ThrowIfNull(timeGroupFunc);

      var obisCodeValues = labelSeries[obisCode];
      var values = obisCodeValues.Where(x => x.End > start && x.End < end).ToList();
      var hourly = new Dictionary<int, UnitValue>(6);
      try
      {
        foreach (var grouping in values.GroupBy(timeGroupFunc, sv => sv))
        {
          hourly.Add(grouping.Key, grouping.Select(sv => sv.UnitValue).Sum());
        }
      }
      catch (DataMisalignedException e)
      {
        logger.LogInformation(e, "Unable to check for leak characteristic. Data error");
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
