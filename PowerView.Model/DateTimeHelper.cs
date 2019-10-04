using System;
using System.Globalization;

namespace PowerView.Model
{
  public class DateTimeHelper
  {
    private readonly TimeZoneInfo timeZoneInfo;
    private readonly DateTime origin;

    public DateTimeHelper(TimeZoneInfo timeZoneInfo, DateTime origin)
    {
      if (timeZoneInfo == null) throw new ArgumentNullException("timeZoneInfo");
      if (origin.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("origin", "Must be UTC");
      var originAtTimeZone = TimeZoneInfo.ConvertTimeFromUtc(origin, timeZoneInfo);
      if (originAtTimeZone.TimeOfDay != TimeSpan.Zero) throw new ArgumentOutOfRangeException("origin", "Must be midnight for timezone. Was:" + originAtTimeZone.ToString("o"));

      this.timeZoneInfo = timeZoneInfo;
      this.origin = origin;
    }

    public DateTime GetPeriodEnd(string period)
    {
      if (period == null) throw new ArgumentNullException("period");
      if (period != "day" && period != "month" && period != "year") throw new ArgumentOutOfRangeException("period", "Must be: day, month or year. Was:" + period);

      DateTime end;
      switch (period)
      {
        case "day":
          end = origin.AddDays(1);
          break;

        case "month":
          end = NextMonth(origin);
          break;

        case "year":
          end = origin.AddYears(1);
          break;

        default:
          throw new NotSupportedException();
      }

      return AdjustForDst(timeZoneInfo, origin, end);
    }

    private static DateTime AdjustForDst(TimeZoneInfo timeZoneInfo, DateTime previous, DateTime next)
    {
      var previousIsDst = timeZoneInfo.IsDaylightSavingTime(previous);
      var nextIsDst = timeZoneInfo.IsDaylightSavingTime(next);
      if (previousIsDst != nextIsDst)
      {
        if (previousIsDst) next += TimeSpan.FromHours(1); else next -= TimeSpan.FromHours(1);
      }
      return next;
    }

    /// <summary>
    /// returns next month's date. When the current date is the last day of the month, it will return the last day of next month.
    /// </summary>
    private static DateTime NextMonth(DateTime date)
    {
      return date.Day != DateTime.DaysInMonth(date.Year, date.Month) ? date.AddMonths(1) : date.AddDays(1).AddMonths(1).AddDays(-1);
    }


    public Func<DateTime, DateTime> GetNext(string interval)
    {
      var intervalElements = SplitInterval(interval);

      switch (intervalElements[1])
      {
        case "minutes":
          var minutes = TimeSpan.FromMinutes(ToDouble(intervalElements[0]));
          if (minutes.TotalHours > 1 || (minutes.Hours == 0 && minutes.Minutes == 0) || minutes.Milliseconds != 0) throw new ArgumentOutOfRangeException("interval", interval, "Minute part invalid");
          return dt => 
          {
            var next = dt.Add(minutes);
            return AdjustForDst(timeZoneInfo, dt, next);
          };

        case "days":
          if (ToInt32(intervalElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Day part invalid");
          return dt =>
          {
            var next = dt.AddDays(1);
            return AdjustForDst(timeZoneInfo, dt, next);
          };

        case "months":
          if (ToInt32(intervalElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Month part invalid");
          return dt =>
          {
            var next = NextMonth(dt);
            return AdjustForDst(timeZoneInfo, dt, next);
          };

        default:
          throw new NotSupportedException();
      }
    }

    public Func<DateTime, DateTime> GetDivider(string interval)
    {
      var intervalElements = SplitInterval(interval);

      switch (intervalElements[1])
      {
        case "minutes":
          var minutes = TimeSpan.FromMinutes(ToDouble(intervalElements[0]));
          if (minutes.TotalHours > 1 || (minutes.Hours == 0 && minutes.Minutes == 0) || minutes.Milliseconds != 0) throw new ArgumentOutOfRangeException("interval", interval, "Minute part invalid");
          return dt =>
          {
            var divided = Divide(dt, origin, minutes);
            return AdjustForDst(timeZoneInfo, origin, divided);
          };

        case "days":
          var days = TimeSpan.FromDays(ToDouble(intervalElements[0]));
          if ((int)days.TotalDays != 1) throw new ArgumentOutOfRangeException("interval", interval, "Day part invalid");
          return dt =>
          {
            var divided = Divide(dt, origin, days);
            return AdjustForDst(timeZoneInfo, origin, divided);
          };

        case "months":
          if (ToInt32(intervalElements[0]) != 1) throw new ArgumentOutOfRangeException("interval", interval, "Month part invalid");
          var lastDayOfMonth = origin.Day == DateTime.DaysInMonth(origin.Year, origin.Month);
          return dt =>
          {
            var year = dt.Year;
            var month = dt.Month;
            if (dt.TimeOfDay < origin.TimeOfDay || 
                (dt.TimeOfDay < origin.TimeOfDay + TimeSpan.FromHours(1) && !timeZoneInfo.IsDaylightSavingTime(dt) && timeZoneInfo.IsDaylightSavingTime(origin)) )
            {
              month--;
              if (month == 0)
              {
                month = 12;
                year--;
              }
            }

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var day = lastDayOfMonth ? daysInMonth : Math.Min(origin.Day, daysInMonth);

            var divided = new DateTime(year, month, day, origin.Hour, origin.Minute, origin.Second, origin.Millisecond, dt.Kind);
            var adjusted = AdjustForDst(timeZoneInfo, origin, divided);
            return adjusted;
          };

        default:
          throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");
      }
    }

    private static string[] SplitInterval(string interval)
    {
      if (interval == null) throw new ArgumentNullException("interval");

      var intervalElements = interval.Split(new[] { '-' }, StringSplitOptions.None);
      if (intervalElements.Length != 2) throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");

      if (intervalElements[1] != "minutes" && intervalElements[1] != "days" && intervalElements[1] != "months")
      {
        throw new ArgumentOutOfRangeException("interval", interval, "Unknown interval");
      }

      return intervalElements;
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
