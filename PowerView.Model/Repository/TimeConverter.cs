using System;

namespace PowerView.Model.Repository
{
  public class TimeConverter : ITimeConverter
  {
    private readonly TimeZoneInfo timeZoneInfo;

    public TimeConverter(ILocationProvider timeZoneProvider)
    {
      if (timeZoneProvider == null) throw new ArgumentNullException("timeZoneProvider");

      timeZoneInfo = timeZoneProvider.GetTimeZone();
    }

    #region ITimeConverter implementation

    public DateTime ChangeTimeZoneFromUtc(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC");

      return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo);
    }

    public DateTime ChangeTimeZoneToUtc(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException("dateTime", "Must be Unspecified");

      return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZoneInfo);
    }

    public DateTime Reduce(DateTime dateTime, DateTimeResolution resolution)
    {
      switch (resolution)
      {
        case DateTimeResolution.Day:
          return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, dateTime.Kind);

        case DateTimeResolution.Month:
          return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, 0, dateTime.Kind);

        case DateTimeResolution.Year:
          return new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind);

        default:
          throw new NotSupportedException(resolution + " not supported. Extend this method!");
      }
    }

    public bool IsGreaterThanResolutionFraction(DateTimeResolution resolution, double fraction, DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC");

      var zonedDateTime = ChangeTimeZoneFromUtc(dateTime);
      var baseDateTime = Reduce(zonedDateTime, resolution);
      var progression = zonedDateTime - baseDateTime;

      TimeSpan maxTimeSpan;
      switch (resolution)
      {
        case DateTimeResolution.Day:
          maxTimeSpan = TimeSpan.FromDays(1);
          break;

        case DateTimeResolution.Month:
          maxTimeSpan = TimeSpan.FromDays(DateTime.DaysInMonth(zonedDateTime.Year, zonedDateTime.Month));
          break;

        case DateTimeResolution.Year:
          var zonedNewYear = new DateTime(zonedDateTime.Year, 1, 1, 0, 0, 0, zonedDateTime.Kind);
          maxTimeSpan = zonedNewYear.AddYears(1) - zonedNewYear;
          break;

        default:
          throw new NotSupportedException(resolution + " not supported. Extend this method!");
      }

      return progression.TotalMilliseconds > maxTimeSpan.TotalMilliseconds * fraction;
    }

    #endregion
  }
}

