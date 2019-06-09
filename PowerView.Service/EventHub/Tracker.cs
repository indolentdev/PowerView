using System;

namespace PowerView.Service.EventHub
{
  internal class Tracker : ITracker
  {
    private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(2);

    private readonly IFactory factory;

    private DateTime lastRun;

    public Tracker(IFactory factory)
      : this(factory, DateTime.Now)
    {
    }

    internal Tracker(IFactory factory, DateTime dateTime)
    {
      if (factory == null) throw new ArgumentNullException("factory");
      if (dateTime.Kind != DateTimeKind.Local) throw new ArgumentOutOfRangeException("dateTime");

      this.factory = factory;

      lastRun = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 12, 0, 0, 0, dateTime.Kind);
    }

    public void Track(DateTime dateTime)
    {
      if (dateTime < lastRun + minimumDayInterval)
      {
        return;
      }
      lastRun = GetDay(dateTime, lastRun);

      using (var usageMonitor = factory.Create<IUsageMonitor>())
      {
        usageMonitor.Value.TrackDing();
      }
    }

    private static DateTime GetDay(DateTime dt, DateTime lastRun)
    {
      var day = TimeSpan.FromDays(1);
      while (lastRun.Date < dt.Date)
      {
        lastRun += day;
      }
      return lastRun;
    }

  }
}
