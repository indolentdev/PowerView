using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  internal static class ValueExpressionSetHelper
  {
    internal static DateTime GetMeanTimestamp(params TimeRegisterValue[] timeRegisterValues)
    {
      return GetMeanTimestamp(timeRegisterValues.Select(sv => sv.Timestamp));
    }

    private static DateTime GetMeanTimestamp(IEnumerable<DateTime> timestampsUnordered)
    {      
      var timestamps = timestampsUnordered.OrderBy(ts => ts).ToArray();
      var first = timestamps.First();
      var second = timestamps.Skip(1).FirstOrDefault();
      if (second == DateTime.MinValue)
      {
        return first;
      }

      var diff = second - first;
      var mean = first + TimeSpan.FromSeconds(diff.TotalSeconds / 2);
      var meanTimestamp = new DateTime(mean.Year, mean.Month, mean.Day, mean.Hour, mean.Minute, mean.Second, mean.Kind);

      return timestamps.Length <= 2 ? meanTimestamp : GetMeanTimestamp(new[] {meanTimestamp}.Concat(timestamps.Skip(2)));
    }
  }
}

