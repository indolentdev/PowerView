using System;
using System.Linq;

namespace PowerView.Model.Test
{
  public static class TimeZoneHelper
  {
    public static TimeZoneInfo GetTimeZoneInfo()
    {
      var timeZones = TimeZoneInfo.GetSystemTimeZones();
      var linuxTzi = timeZones.FirstOrDefault(x => x.Id == "Europe/Copenhagen");
      if (linuxTzi != null) return linuxTzi;
      var windowsTzi = timeZones.FirstOrDefault(x => x.Id == "Romance Standard Time");
      if (windowsTzi != null) return windowsTzi;

      throw new NotImplementedException("Stuff is missing...");
    }

  }
}
