using System;
using System.Linq;

namespace PowerView.Service.Test
{
  public static class TimeZoneHelper
  {
    public static TimeZoneInfo GetDenmarkTimeZoneInfo()
    {
      var timeZones = TimeZoneInfo.GetSystemTimeZones();
      var linuxTzi = timeZones.FirstOrDefault(x => x.Id == "Europe/Copenhagen");
      if (linuxTzi != null) return linuxTzi;
      var windowsTzi = timeZones.FirstOrDefault(x => x.Id == "Romance Standard Time");
      if (windowsTzi != null) return windowsTzi;

      throw new NotImplementedException("Stuff is missing...");
    }

    public static DateTime GetDenmarkTodayAsUtc()
    {
      var utcNow = DateTime.UtcNow;
      var timeZoneNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, GetDenmarkTimeZoneInfo());
      return timeZoneNow.Date.ToUniversalTime();
    }

    public static ILocationContext GetDenmarkLocationContext()
    {
      var locationContext = new LocationContext();
      locationContext.Setup(GetDenmarkTimeZoneInfo(), new System.Globalization.CultureInfo("da-DK"));
      return locationContext;
    }
  }
}
