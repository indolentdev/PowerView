using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public static class DeviceId
  {
    public static bool Equals(string deviceId1, string deviceId2)
    {
#pragma warning disable CA1309 // Use ordinal string comparison      
      return string.Equals(deviceId1, deviceId2, StringComparison.InvariantCultureIgnoreCase);
#pragma warning restore CA1309 // Use ordinal string comparison      
    }

    public static string[] DistinctDeviceIds(params IEnumerable<string>[] strings)
    {
      if (strings.Length == 0)
      {
        return Array.Empty<string>();
      }

      var stringsConcat = strings[0];
      for (var i=1; i<strings.Length; i++)
      {
        stringsConcat = stringsConcat.Concat(strings[i]);
      }

      return stringsConcat.Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();
    }
  }
}
