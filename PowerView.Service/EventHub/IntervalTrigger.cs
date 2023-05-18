using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
    public class IntervalTrigger : IIntervalTrigger
    {
        private readonly ILogger logger;
        private readonly ILocationContext locationContext;
        private readonly DateTime baseDateTime;

        private TimeSpan interval;
        private TimeSpan timeOfDayAtTimezone;

        private DateTime? lastRunAtTimezone;

        public IntervalTrigger(ILogger<IntervalTrigger> logger, ILocationContext locationContext)
          : this(logger, locationContext, DateTime.UtcNow)
        {
        }

        internal IntervalTrigger(ILogger<IntervalTrigger> logger, ILocationContext locationContext, DateTime baseDateTime)
        {
            if (baseDateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
            this.baseDateTime = baseDateTime;

            logger.LogDebug("Interval trigger initialized. Base date time:{0}", baseDateTime.ToString("O"));
        }

        public void Setup(TimeSpan timeOfDayAtTimezone, TimeSpan interval)
        {
            if (timeOfDayAtTimezone.TotalHours >= 24) throw new ArgumentOutOfRangeException("timeOfDayAtTimezone", "Must not be greater than 24 hours. Was:" + timeOfDayAtTimezone);

            this.timeOfDayAtTimezone = timeOfDayAtTimezone;
            this.interval = interval;

            var baseAtTimezone = locationContext.ConvertTimeFromUtc(baseDateTime);
            lastRunAtTimezone = new DateTime(baseAtTimezone.Year, baseAtTimezone.Month, baseAtTimezone.Day, 0, 0, 0, 0).Add(timeOfDayAtTimezone);
            logger.LogDebug("Interval trigger Setup. Last run date time:{0}. Interval:{1}", lastRunAtTimezone.Value.ToString("O"), interval);
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

            while (lastRunAtTimezone.Value < dateTimeAtTimezone)
            {
                lastRunAtTimezone += interval;
            }
        }

    }
}
