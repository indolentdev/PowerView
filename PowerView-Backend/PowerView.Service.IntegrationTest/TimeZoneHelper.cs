using System;
using System.Linq;
using PowerView.Model;

namespace PowerView.Service.IntegrationTest
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
      return GetDenmarkMidnightAsUtc(utcNow);
    }

    public static DateTime GetDenmarkFixedMidnightAsUtc()
    {
      var utc = new DateTime(2020, 3, 27, 13, 22, 1, DateTimeKind.Utc);
      return GetDenmarkMidnightAsUtc(utc);
    }

    public static DateTime GetDenmarkMidnightAsUtc(DateTime utc)
    {
      var timeZoneMidnight = TimeZoneInfo.ConvertTimeFromUtc(utc, GetDenmarkTimeZoneInfo());
      return timeZoneMidnight.Date.ToUniversalTime();
    }

    public static ILocationContext GetDenmarkLocationContext()
    {
      var locationContext = new LocationContext();
      locationContext.Setup(GetDenmarkTimeZoneInfo(), new System.Globalization.CultureInfo("da-DK"));
      return locationContext;
    }
  }
}
