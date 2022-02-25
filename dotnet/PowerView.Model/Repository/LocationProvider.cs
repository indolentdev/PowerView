using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace PowerView.Model.Repository
{
  internal class LocationProvider : ILocationProvider
  {
    private readonly ILogger<LocationProvider> logger;
    private readonly ISettingRepository settingRepository;
    private readonly string configuredTimeZone;
    private readonly string configuredCultureInfo;

    public LocationProvider(ILogger<LocationProvider> logger, ISettingRepository settingRepository, string configuredTimeZone, string configuredCultureInfo)
    {
      if (settingRepository == null) throw new ArgumentNullException("settingRepository");

      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
          logger.LogDebug($"Resolved time zone from configuration: {configTimeZoneInfo.Id}:{configTimeZoneInfo.DisplayName}");
          return configTimeZoneInfo;
        }
      }

      var timeZoneId = settingRepository.Get(Settings.TimeZoneId);
      if (timeZoneId != null)
      {
        var dbTimeZoneInfo = ToTimeZoneInfo(timeZoneId);
        if (dbTimeZoneInfo != null)
        {
          logger.LogDebug($"Resolved time zone from database: {dbTimeZoneInfo.Id}:{dbTimeZoneInfo.DisplayName}");
          return dbTimeZoneInfo;
        }
      }
        
      var timeZoneInfo = ToTimeZoneInfo(TimeZoneInfo.Local.Id);
      logger.LogDebug($"Using operating system time zone: {timeZoneInfo.Id}:{timeZoneInfo.DisplayName}");
      return timeZoneInfo;
    }

    public CultureInfo GetCultureInfo()
    {
      if (!string.IsNullOrEmpty(configuredCultureInfo))
      {
        var configCultureInfo = ToCultureInfo(configuredCultureInfo);
        if (configCultureInfo != null)
        {
          logger.LogDebug($"Resolved culture info from configuration: {configCultureInfo.Name}:{configCultureInfo.EnglishName}");
          return configCultureInfo;
        }
      }

      var cultureInfoName = settingRepository.Get(Settings.CultureInfoName);
      if (cultureInfoName != null)
      {
        var dbCultureInfo = ToCultureInfo(cultureInfoName);
        if (dbCultureInfo != null)
        {
          logger.LogDebug($"Resolved culture info from database: {dbCultureInfo.Name}:{dbCultureInfo.EnglishName}");
          return dbCultureInfo;
        }
      }

      var cultureInfo = CultureInfo.CurrentCulture;
      logger.LogDebug($"Using operating system culture info: {cultureInfo.Name}:{cultureInfo.EnglishName}");
      return cultureInfo;
    }

    private TimeZoneInfo ToTimeZoneInfo(string timeZoneId)
    {
      try
      {
        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
      }
      catch (TimeZoneNotFoundException e)
      {
        logger.LogDebug(e, $"Could not resolve the TimeZone:{timeZoneId}");
      }
      catch (InvalidTimeZoneException e)
      {
        logger.LogDebug(e, $"Invalid TimeZone:{timeZoneId}");
      }

      return null;
    }

    private CultureInfo ToCultureInfo(string cultureInfoName)
    {
      try
      {
        return new CultureInfo(cultureInfoName);
      }
      catch (CultureNotFoundException e)
      {
        logger.LogDebug(e, $"Could not resolve the CultureInfo:{cultureInfoName}");
      }

      return null;
    }

  }
}
