using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace PowerView.Service.EventHub
{
  internal class MeterEventCoordinator : IMeterEventCoordinator
  {
    private readonly ILogger logger;
    private readonly IIntervalTrigger intervalTrigger;
    private readonly IFactory factory;

    public MeterEventCoordinator(ILogger<MeterEventCoordinator> logger, IIntervalTrigger intervalTrigger, IFactory factory)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.intervalTrigger = intervalTrigger ?? throw new ArgumentNullException(nameof(intervalTrigger));
      this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

      this.intervalTrigger.Setup(new TimeSpan(6, 0, 0), TimeSpan.FromDays(1)); // Time of day is discrete dependency on MeterEventDetector... :/
    }

    public void DetectAndNotify(DateTime dateTime)
    {
      if (!intervalTrigger.IsTriggerTime(dateTime))
      {
        return;
      }

      logger.LogDebug("Trigger time occurred. Running Detector. {0}", dateTime.ToString("O"));

      using (var ownedMeterEventDetector = factory.Create<IMeterEventDetector>())
      {
        ownedMeterEventDetector.Value.DetectMeterEvents(dateTime);
      }

      using (var ownedMeterEventNotifier = factory.Create<IMeterEventNotifier>())
      {
        ownedMeterEventNotifier.Value.NotifyEmailRecipients();
      }

      intervalTrigger.Advance(dateTime);
    }

  }
}
