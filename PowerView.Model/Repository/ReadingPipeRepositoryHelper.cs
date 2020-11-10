using System;

namespace PowerView.Model.Repository
{
  internal static class ReadingPipeRepositoryHelper
  {

    // Should this use the DateTimeHelper divider?
    public static DateTime Reduce<TDstReading>(DateTime zonedDateTime)
      where TDstReading : class, IDbReading
    {
      if (zonedDateTime.Kind != DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException("zonedDateTime", "Must be Unspecified");

      var typeName = typeof(TDstReading).Name;
      switch (typeName)
      {
        case "DayReading":
          return new DateTime(zonedDateTime.Year, zonedDateTime.Month, zonedDateTime.Day, 0, 0, 0, 0, zonedDateTime.Kind);

        case "MonthReading":
          return new DateTime(zonedDateTime.Year, zonedDateTime.Month, 1, 0, 0, 0, 0, zonedDateTime.Kind);

        case "YearReading":
          return new DateTime(zonedDateTime.Year, 1, 1, 0, 0, 0, zonedDateTime.Kind);

        default:
          throw new NotSupportedException(typeName + " not supported. Extend this method!");
      }
    }

    public static bool IsGreaterThanResolutionFraction<TDstReading>(double fraction, DateTime zonedDateTime)
      where TDstReading : class, IDbReading
    {
      if (zonedDateTime.Kind != DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException("zonedDateTime", "Must be Unspecified");

      var baseDateTime = Reduce<TDstReading>(zonedDateTime);
      var progression = zonedDateTime - baseDateTime;

      TimeSpan maxTimeSpan;
      var typeName = typeof(TDstReading).Name;
      switch (typeName)
      {
        case "DayReading":
          maxTimeSpan = TimeSpan.FromDays(1);
          break;

        case "MonthReading":
          maxTimeSpan = TimeSpan.FromDays(DateTime.DaysInMonth(zonedDateTime.Year, zonedDateTime.Month));
          break;

        case "YearReading":
          maxTimeSpan = baseDateTime.AddYears(1) - baseDateTime;
          break;

        default:
          throw new NotSupportedException(typeName + " not supported. Extend this method!");
      }

      return progression.TotalMilliseconds > maxTimeSpan.TotalMilliseconds * fraction;
    }
  }
}

