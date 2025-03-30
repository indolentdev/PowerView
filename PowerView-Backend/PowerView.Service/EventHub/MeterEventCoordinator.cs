using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub
{
    internal class MeterEventCoordinator : IMeterEventCoordinator
    {
        private readonly ILogger logger;
        private readonly IIntervalTrigger intervalTrigger;

        public MeterEventCoordinator(ILogger<MeterEventCoordinator> logger, IIntervalTrigger intervalTrigger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.intervalTrigger = intervalTrigger ?? throw new ArgumentNullException(nameof(intervalTrigger));

            this.intervalTrigger.Setup(new TimeSpan(6, 0, 0), TimeSpan.FromDays(1)); // Time of day is discrete dependency on MeterEventDetector... :/
        }

        public void DetectAndNotify(IServiceScope serviceScope, DateTime dateTime)
        {
            if (serviceScope == null) throw new ArgumentNullException(nameof(serviceScope));

            if (!intervalTrigger.IsTriggerTime(dateTime))
            {
                return;
            }

            logger.LogDebug("Trigger time occurred. Running Detector. {0}", dateTime.ToString("O"));

            var meterEventDetector = serviceScope.ServiceProvider.GetRequiredService<IMeterEventDetector>();
            meterEventDetector.DetectMeterEvents(dateTime);

            var meterEventNotifier = serviceScope.ServiceProvider.GetRequiredService<IMeterEventNotifier>();
            meterEventNotifier.NotifyEmailRecipients();

            intervalTrigger.Advance(dateTime);
        }

    }
}
