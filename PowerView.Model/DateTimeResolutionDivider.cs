﻿using System;
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
          return start.AddMonths(1);

        case "year":
          return start.AddYears(1);

        default:
          throw new ArgumentOutOfRangeException("period", "Unsupported period value:" + period);
      }
    }

    public static Func<DateTime, DateTime> GetNext(string interval, bool forward = true)
    {
      if (interval == null) throw new ArgumentNullException("interval");

      var dividerElements = interval.Split(new[] { '-' }, StringSplitOptions.None);
      if (dividerElements.Length != 2) throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");

      var factor = forward ? 1 : -1;

      switch (dividerElements[1])
      {
        case "minutes":
          var minutes = TimeSpan.FromMinutes(ToDouble(dividerElements[0]) * factor);
          if (minutes.TotalHours > 1 || (minutes.Hours == 0 && minutes.Minutes == 0) || minutes.Milliseconds != 0) throw new ArgumentOutOfRangeException("interval", interval, "Minute part invalid");
          return dt => dt.Add(minutes);

        case "days":
          if (ToInt32(dividerElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Day part invalid");
          return dt => dt.AddDays(1 * factor);

        case "months":
          if (ToInt32(dividerElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Month part invalid");
          return dt => dt.AddMonths(1 * factor);

        default:
          throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");
      }
    }

    public static Func<DateTime, DateTime> GetResolutionDivider(string interval)
    {
      if (interval == null) throw new ArgumentNullException("interval");

      var dividerElements = interval.Split(new [] { '-' }, StringSplitOptions.None);
      if (dividerElements.Length != 2) throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");

      switch (dividerElements[1])
      {
        case "minutes":
          var minutes = TimeSpan.FromMinutes(ToDouble(dividerElements[0]));
          if (minutes.TotalHours > 1 || (minutes.Hours == 0 && minutes.Minutes == 0) || minutes.Milliseconds != 0) throw new ArgumentOutOfRangeException("interval", interval, "Minute part invalid");
          return dt => Divide(dt, minutes);

        case "days":
          var days = TimeSpan.FromDays(ToDouble(dividerElements[0]));
          if ((int)days.TotalDays != 1) throw new ArgumentOutOfRangeException("interval", interval, "Day part invalid");
          return dt => Divide(dt, days) + TimeSpan.FromHours(12);

        case "months":
          if (ToInt32(dividerElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Month part invalid");
          return dt => new DateTime(dt.Year, dt.Month, 1, 12, 0, 0, 0, dt.Kind);
          
        default:
          throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");
      }
    }

    private static DateTime Divide(DateTime date, TimeSpan span)
    {
      long ticks = (date.Ticks / span.Ticks) * span.Ticks;
      return new DateTime(ticks, date.Kind);
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
