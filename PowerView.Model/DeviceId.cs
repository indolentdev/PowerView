﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public static class DeviceId
  {
    public static bool Equals(string deviceId1, string deviceId2)
    {
      return string.Equals(deviceId1, deviceId2, StringComparison.InvariantCultureIgnoreCase);
    }

    public static string[] DistinctDeviceIds(params IEnumerable<string>[] strings)
    {
      if (strings.Length == 0)
      {
        return new string[0];
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
