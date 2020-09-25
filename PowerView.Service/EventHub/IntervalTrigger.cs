using System;
using System.Reflection;
using log4net;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
  public class IntervalTrigger : IIntervalTrigger
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ILocationContext locationContext;
    private readonly DateTime baseDateTime;

    private TimeSpan interval;
    private TimeSpan timeOfDayAtTimezone;

    private DateTime? lastRunAtTimezone;

    public IntervalTrigger(ILocationContext locationContext)
      : this(locationContext, DateTime.UtcNow)
    {
    }

    internal IntervalTrigger(ILocationContext locationContext, DateTime baseDateTime)
    {
      if (locationContext == null) throw new ArgumentNullException("locationContext");
      if (baseDateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");

      this.locationContext = locationContext;
      this.baseDateTime = baseDateTime;

      log.DebugFormat("Interval trigger initialized. Base date time:{0}", baseDateTime.ToString("O"));
    }

    public void Setup(TimeSpan timeOfDayAtTimezone, TimeSpan interval)
    {
      if (timeOfDayAtTimezone.TotalHours >= 24) throw new ArgumentOutOfRangeException("timeOfDayAtTimezone", "Must not be greater than 24 hours. Was:" + timeOfDayAtTimezone);

      this.timeOfDayAtTimezone = timeOfDayAtTimezone;
      this.interval = interval;

      var baseAtTimezone = locationContext.ConvertTimeFromUtc(baseDateTime);
      lastRunAtTimezone = new DateTime(baseAtTimezone.Year, baseAtTimezone.Month, baseAtTimezone.Day, 0, 0, 0, 0).Add(timeOfDayAtTimezone);
    }

    public bool IsTriggerTime(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");
      if (lastRunAtTimezone == null) throw new InvalidOperationException("Setup first");

      var dateTimeAtTimezone = locationContext.ConvertTimeFromUtc(dateTime);

      if (dateTimeAtTimezone < lastRunAtTimezone.Value + interval)
      {
        return false;
      }

      return true;
    }

    public void Advance(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");
      if (lastRunAtTimezone == null) throw new InvalidOperationException("Setup first");

      var dateTimeAtTimezone = locationContext.ConvertTimeFromUtc(dateTime);

      while (lastRunAtTimezone.Value.Date < dateTimeAtTimezone.Date)
      {
        lastRunAtTimezone += interval;
      }
    }

  }
}
