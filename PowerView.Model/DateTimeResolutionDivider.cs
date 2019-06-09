using System;
using System.Globalization;

namespace PowerView.Model
{
  internal static class DateTimeResolutionDivider
  {
    public static Func<DateTime, DateTime> GetResolutionDivider(string interval)
    {
      if (interval == null) throw new ArgumentNullException("interval");

      var dividerElements = interval.Split(new [] { '-' }, StringSplitOptions.None);
      if (dividerElements.Length != 2) throw new ArgumentOutOfRangeException("interval", interval, "Unknown intervall");

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
