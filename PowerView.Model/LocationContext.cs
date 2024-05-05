using System;
using System.Globalization;

namespace PowerView.Model
{
    public class LocationContext : ILocationContext
    {
        public void Setup(TimeZoneInfo timeZoneInfo, CultureInfo cultureInfo)
        {
            if (timeZoneInfo == null) throw new ArgumentNullException("timeZoneInfo");
            if (cultureInfo == null) throw new ArgumentNullException("cultureInfo");

            TimeZoneInfo = timeZoneInfo;
            CultureInfo = cultureInfo;
        }

        public TimeZoneInfo TimeZoneInfo { get; private set; }
        public CultureInfo CultureInfo { get; private set; }

        public DateTime ConvertTimeFromUtc(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC. Was:" + dateTime.Kind);

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo);
        }

        public DateTime ConvertTimeToUtc(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException("dateTime", "Must be Unspecified");

            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo);
        }

        public bool IsDaylightSavingTime(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC. Was:" + dateTime.Kind);

            return TimeZoneInfo.IsDaylightSavingTime(dateTime);
        }


    }
}
