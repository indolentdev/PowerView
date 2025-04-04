﻿using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PowerView.Model.Repository
{
    internal class LocationProvider : ILocationProvider
    {
        private readonly ILogger<LocationProvider> logger;
        private readonly IOptions<DatabaseRegionOptions> options;
        private readonly ISettingRepository settingRepository;

        public LocationProvider(ILogger<LocationProvider> logger, IOptions<DatabaseRegionOptions> options, ISettingRepository settingRepository)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
        }

        public TimeZoneInfo GetTimeZone()
        {
            var configuredTimeZone = options.Value.TimeZone;
            if (!string.IsNullOrEmpty(configuredTimeZone))
            {
                var configTimeZoneInfo = ToTimeZoneInfo(configuredTimeZone);
                if (configTimeZoneInfo != null)
                {
                    logger.LogInformation("Resolved time zone from configuration: {TzId}:{Name}", configTimeZoneInfo.Id, configTimeZoneInfo.DisplayName);
                    return configTimeZoneInfo;
                }
            }

            var timeZoneId = settingRepository.Get(Settings.TimeZoneId);
            if (timeZoneId != null)
            {
                var dbTimeZoneInfo = ToTimeZoneInfo(timeZoneId);
                if (dbTimeZoneInfo != null)
                {
                    logger.LogInformation("Resolved time zone from database: {TzId}:{Name}", dbTimeZoneInfo.Id, dbTimeZoneInfo.DisplayName);
                    return dbTimeZoneInfo;
                }
            }

            var timeZoneInfo = ToTimeZoneInfo(TimeZoneInfo.Local.Id);
            logger.LogInformation("Resolved time zone from operatng system: {TzId}:{Name}", timeZoneInfo.Id, timeZoneInfo.DisplayName);
            return timeZoneInfo;
        }

        public CultureInfo GetCultureInfo()
        {
            var configuredCultureInfo = options.Value.CultureInfo;
            if (!string.IsNullOrEmpty(configuredCultureInfo))
            {
                var configCultureInfo = ToCultureInfo(configuredCultureInfo);
                if (configCultureInfo != null)
                {
                    logger.LogInformation("Resolved culture info from configuration: {Name}:{EnglishName}", configCultureInfo.Name, configCultureInfo.EnglishName);
                    return configCultureInfo;
                }
            }

            var cultureInfoName = settingRepository.Get(Settings.CultureInfoName);
            if (cultureInfoName != null)
            {
                var dbCultureInfo = ToCultureInfo(cultureInfoName);
                if (dbCultureInfo != null)
                {
                    logger.LogInformation("Resolved culture info from database: {Name}:{EnglishName}", dbCultureInfo.Name, dbCultureInfo.EnglishName);
                    return dbCultureInfo;
                }
            }

            var cultureInfo = CultureInfo.CurrentCulture;
            logger.LogInformation("Resolved culture info from operating system: {Name}:{EnglishName}", cultureInfo.Name, cultureInfo.EnglishName);
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
                logger.LogInformation(e, "Could not resolve the TimeZone:{TzId}", timeZoneId);
            }
            catch (InvalidTimeZoneException e)
            {
                logger.LogInformation(e, "Invalid TimeZone:{TzId}", timeZoneId);
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
                logger.LogInformation(e, "Could not resolve the CultureInfo:{Name}", cultureInfoName);
            }

            return null;
        }

    }
}
