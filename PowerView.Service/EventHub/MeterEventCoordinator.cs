using System;

namespace PowerView.Service.EventHub
{
  internal class MeterEventCoordinator : IMeterEventCoordinator
  {
    private readonly IIntervalTrigger intervalTrigger;
    private readonly IFactory factory;

    public MeterEventCoordinator(IIntervalTrigger intervalTrigger, IFactory factory)
    {
      if (intervalTrigger == null) throw new ArgumentNullException("intervalTrigger");
      if (factory == null) throw new ArgumentNullException("factory");

      this.intervalTrigger = intervalTrigger;
      this.factory = factory;

      this.intervalTrigger.Setup(new TimeSpan(6, 0, 0), TimeSpan.FromDays(1)); // Time of day is discrete dependency on MeterEventDetector... :/
    }

    public void DetectAndNotify(DateTime dateTime)
    {
      if (!intervalTrigger.IsTriggerTime(dateTime))
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

      intervalTrigger.Advance(dateTime);
    }

  }
}
