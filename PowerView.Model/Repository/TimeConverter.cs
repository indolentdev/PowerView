using System;

namespace PowerView.Model.Repository
{
  internal static class TimeConverter
  {
    public static DateTime Reduce(DateTimeResolution resolution, DateTime zonedDateTime)
    {
      if (zonedDateTime.Kind != DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException("zonedDateTime", "Must be Unspecified");

      switch (resolution)
      {
        case DateTimeResolution.Day:
          return new DateTime(zonedDateTime.Year, zonedDateTime.Month, zonedDateTime.Day, 0, 0, 0, 0, zonedDateTime.Kind);

        case DateTimeResolution.Month:
          return new DateTime(zonedDateTime.Year, zonedDateTime.Month, 1, 0, 0, 0, 0, zonedDateTime.Kind);

        case DateTimeResolution.Year:
          return new DateTime(zonedDateTime.Year, 1, 1, 0, 0, 0, zonedDateTime.Kind);

        default:
          throw new NotSupportedException(resolution + " not supported. Extend this method!");
      }
    }

    public static bool IsGreaterThanResolutionFraction(DateTimeResolution resolution, double fraction, DateTime zonedDateTime)
    {
      if (zonedDateTime.Kind != DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException("zonedDateTime", "Must be Unspecified");

      var baseDateTime = Reduce(resolution, zonedDateTime);
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
          maxTimeSpan = baseDateTime.AddYears(1) - baseDateTime;
          break;

        default:
          throw new NotSupportedException(resolution + " not supported. Extend this method!");
      }

      return progression.TotalMilliseconds > maxTimeSpan.TotalMilliseconds * fraction;
    }
  }

  internal enum DateTimeResolution
  {
    Day,
    Month,
    Year
  }

}

