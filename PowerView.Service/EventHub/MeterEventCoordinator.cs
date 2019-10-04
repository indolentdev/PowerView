using System;

namespace PowerView.Service.EventHub
{
  internal class MeterEventCoordinator : IMeterEventCoordinator
  {
    private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(1);

    private readonly IFactory factory;

    private DateTime lastRun;

    public MeterEventCoordinator(IFactory factory)
      : this(factory, DateTime.Now)
    {
    }

    internal MeterEventCoordinator(IFactory factory, DateTime dateTime)
    {
      if (factory == null) throw new ArgumentNullException("factory");
      if (dateTime.Kind != DateTimeKind.Local) throw new ArgumentOutOfRangeException("dateTime");

      this.factory = factory;

      lastRun = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 6, 0, 0, 0, dateTime.Kind); // Time is discrete dependency on MeterEventDetector... :/
    }

    public void DetectAndNotify(DateTime dateTime)
    {
      if (dateTime < lastRun + minimumDayInterval)
      {
        return;
      }

      var dateTimeUtc = dateTime.Date.ToUniversalTime();

      using (var ownedMeterEventDetector = factory.Create<IMeterEventDetector>())
      {
        ownedMeterEventDetector.Value.DetectMeterEvents(dateTimeUtc);
      }

      using (var ownedMeterEventNotifier = factory.Create<IMeterEventNotifier>())
      {
        ownedMeterEventNotifier.Value.NotifyEmailRecipients();
      }

      lastRun = GetDay(dateTime, lastRun);
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
