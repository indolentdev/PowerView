using System;
using System.Globalization;
using System.Reflection;
using log4net;

namespace PowerView.Model.Repository
{
  internal class LocationProvider : ILocationProvider
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ISettingRepository settingRepository;
    private readonly string configuredTimeZone;
    private readonly string configuredCultureInfo;

    public LocationProvider(ISettingRepository settingRepository, string configuredTimeZone, string configuredCultureInfo)
    {
      if (settingRepository == null) throw new ArgumentNullException("settingRepository");

      this.settingRepository = settingRepository;
      this.configuredTimeZone = configuredTimeZone;
      this.configuredCultureInfo = configuredCultureInfo;
    }

    public TimeZoneInfo GetTimeZone()
    {
      if (!string.IsNullOrEmpty(configuredTimeZone))
      {
        var configTimeZoneInfo = ToTimeZoneInfo(configuredTimeZone);
        if (configTimeZoneInfo != null)
        {
          log.DebugFormat("Resolved time zone from configuration: {0}:{1}", configTimeZoneInfo.Id, configTimeZoneInfo.DisplayName);
          return configTimeZoneInfo;
        }
      }

      var timeZoneId = settingRepository.Get(Settings.TimeZoneId);
      if (timeZoneId != null)
      {
        var dbTimeZoneInfo = ToTimeZoneInfo(timeZoneId);
        if (dbTimeZoneInfo != null)
        {
          log.DebugFormat("Resolved time zone from database: {0}:{1}", dbTimeZoneInfo.Id, dbTimeZoneInfo.DisplayName);
          return dbTimeZoneInfo;
        }
      }
        
      var timeZoneInfo = ToTimeZoneInfo(TimeZoneInfo.Local.Id);
      log.DebugFormat("Using operating system time zone: {0}:{1}", timeZoneInfo.Id, timeZoneInfo.DisplayName);
      return timeZoneInfo;
    }

    public CultureInfo GetCultureInfo()
    {
      if (!string.IsNullOrEmpty(configuredCultureInfo))
      {
        var configCultureInfo = ToCultureInfo(configuredCultureInfo);
        if (configCultureInfo != null)
        {
          log.DebugFormat("Resolved culture info from configuration: {0}:{1}", configCultureInfo.Name, configCultureInfo.EnglishName);
          return configCultureInfo;
        }
      }

      var cultureInfoName = settingRepository.Get(Settings.CultureInfoName);
      if (cultureInfoName != null)
      {
        var dbCultureInfo = ToCultureInfo(cultureInfoName);
        if (dbCultureInfo != null)
        {
          log.DebugFormat("Resolved culture info from database: {0}:{1}", dbCultureInfo.Name, dbCultureInfo.EnglishName);
          return dbCultureInfo;
        }
      }

      var cultureInfo = CultureInfo.CurrentCulture;
      log.DebugFormat("Using operating system culture info: {0}:{1}", cultureInfo.Name, cultureInfo.EnglishName);
      return cultureInfo;
    }

    private static TimeZoneInfo ToTimeZoneInfo(string timeZoneId)
    {
      try
      {
        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
      }
      catch (TimeZoneNotFoundException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Could not resolve the TimeZone:{0}", timeZoneId);
        log.Debug(msg, e);
      }
      catch (InvalidTimeZoneException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Invalid TimeZone:{0}", timeZoneId);
        log.Debug(msg, e);
      }

      return null;
    }

    private static CultureInfo ToCultureInfo(string cultureInfoName)
    {
      try
      {
        return new CultureInfo(cultureInfoName);
      }
      catch (CultureNotFoundException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Could not resolve the CultureInfo:{0}", cultureInfoName);
        log.Debug(msg, e);
      }

      return null;
    }

  }
}
