using System;
using System.Globalization;

namespace PowerView.Model
{
    public class LocationContext : ILocationContext
    {
        public void Setup(TimeZoneInfo timeZoneInfo, CultureInfo cultureInfo)
        {
            ArgumentNullException.ThrowIfNull(timeZoneInfo);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            TimeZoneInfo = timeZoneInfo;
            CultureInfo = cultureInfo;
        }

        public TimeZoneInfo TimeZoneInfo { get; private set; }
        public CultureInfo CultureInfo { get; private set; }

        public string GetTimeZoneDisplayName()
        {
            return TimeZoneInfo.DisplayName;
        }

        public DateTime ConvertTimeFromUtc(DateTime dateTime)
        {
            ArgCheck.ThrowIfNotUtc(dateTime);

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo);
        }

        public DateTime ConvertTimeToUtc(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException(nameof(dateTime), "Must be Unspecified");

            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo);
        }

        public bool IsDaylightSavingTime(DateTime dateTime)
        {
            ArgCheck.ThrowIfNotUtc(dateTime);

            return TimeZoneInfo.IsDaylightSavingTime(dateTime);
        }


    }
}
