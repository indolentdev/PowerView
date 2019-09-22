using System;
using System.Globalization;

namespace PowerView.Model
{
  public static class DateTimeResolutionDivider
  {
    public static DateTime GetPeriodEnd(string period, DateTime start)
    {
      if (period == null) throw new ArgumentNullException("period");
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");

      switch (period)
      {
        case "day":
          return start.AddDays(1);

        case "month":
          return NextMonth(start);

        case "year":
          return start.AddYears(1);

        default:
          throw new ArgumentOutOfRangeException("period", "Unsupported period value:" + period);
      }
    }

    /// <summary>
    /// returns next month's date. When the current date is the last day of the month, it will return the last day of next month.
    /// </summary>
    private static DateTime NextMonth(DateTime date)
    {
      return date.Day != DateTime.DaysInMonth(date.Year, date.Month) ? date.AddMonths(1) : date.AddDays(1).AddMonths(1).AddDays(-1);
    }

    public static Func<DateTime, DateTime> GetNext(string interval)
    {
      if (interval == null) throw new ArgumentNullException("interval");

      var dividerElements = interval.Split(new[] { '-' }, StringSplitOptions.None);
      if (dividerElements.Length != 2) throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");

      switch (dividerElements[1])
      {
        case "minutes":
          var minutes = TimeSpan.FromMinutes(ToDouble(dividerElements[0]));
          if (minutes.TotalHours > 1 || (minutes.Hours == 0 && minutes.Minutes == 0) || minutes.Milliseconds != 0) throw new ArgumentOutOfRangeException("interval", interval, "Minute part invalid");
          return dt => dt.Add(minutes);

        case "days":
          if (ToInt32(dividerElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Day part invalid");
          return dt => dt.AddDays(1);

        case "months":
          if (ToInt32(dividerElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Month part invalid");
          return dt => NextMonth(dt);

        default:
          throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");
      }
    }

    public static Func<DateTime, DateTime> GetResolutionDivider(DateTime origin, string interval)
    {
      if (origin.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("origin", "Must be UTC");
      if (interval == null) throw new ArgumentNullException("interval");

      var dividerElements = interval.Split(new[] { '-' }, StringSplitOptions.None);
      if (dividerElements.Length != 2) throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");

      switch (dividerElements[1])
      {
        case "minutes":
          var minutes = TimeSpan.FromMinutes(ToDouble(dividerElements[0]));
          if (minutes.TotalHours > 1 || (minutes.Hours == 0 && minutes.Minutes == 0) || minutes.Milliseconds != 0) throw new ArgumentOutOfRangeException("interval", interval, "Minute part invalid");
          return dt => Divide(dt, origin, minutes);

        case "days":
          var days = TimeSpan.FromDays(ToDouble(dividerElements[0]));
          if ((int)days.TotalDays != 1) throw new ArgumentOutOfRangeException("interval", interval, "Day part invalid");
          return dt => Divide(dt, origin, days);

        case "months":
          if (ToInt32(dividerElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Month part invalid");
          var lastDayOfMonth = origin.Day == DateTime.DaysInMonth(origin.Year, origin.Month);
          return dt =>
          {
            var year = dt.Year;
            var month = dt.Month;
            if (dt.TimeOfDay < origin.TimeOfDay)
            {
              month--;
              if (month == 0)
              {
                month = 12;
                year--;
              }
            }

            var daysInMonth = DateTime.DaysInMonth(dt.Year, month);
            var day = lastDayOfMonth ? daysInMonth : Math.Min(origin.Day, daysInMonth);

            return new DateTime(year, month, day, origin.Hour, origin.Minute, origin.Second, origin.Millisecond, dt.Kind);
          };

        default:
          throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");
      }
    }

    private static DateTime Divide(DateTime date, DateTime origin, TimeSpan span)
    {
      var ticksDiff = date.Ticks - origin.Ticks;
      if (ticksDiff < 0)
      {
        ticksDiff -= span.Ticks;
      }
      long ticks = (ticksDiff / span.Ticks) * span.Ticks;
      return new DateTime(origin.Ticks + ticks, date.Kind);
    }

    private static int ToInt32(string s)
    {
      return int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    private static double ToDouble(string s)
    {
      return double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
  }
}
