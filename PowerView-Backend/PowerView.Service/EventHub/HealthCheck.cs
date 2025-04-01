using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
    internal class HealthCheck : IHealthCheck
    {
        private readonly ILogger logger;
        private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(1);

        private readonly IIntervalTrigger intervalTrigger;

        public HealthCheck(ILogger<HealthCheck> logger, IIntervalTrigger intervalTrigger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.intervalTrigger = intervalTrigger ?? throw new ArgumentNullException(nameof(intervalTrigger));

            this.intervalTrigger.Setup(new TimeSpan(0, 15, 0), TimeSpan.FromDays(1));
        }

        public void DailyCheck(IServiceScope serviceScope, DateTime dateTime)
        {
            ArgumentNullException.ThrowIfNull(serviceScope);

            if (!intervalTrigger.IsTriggerTime(dateTime))
            {
                return;
            }

            var dbCheck = serviceScope.ServiceProvider.GetRequiredService<IDbCheck>();
            logger.LogInformation("Performing database check");
            try
            {
                dbCheck.CheckDatabase();
                logger.LogInformation("Database check completed");
            }
            catch (DataStoreCorruptException e)
            {
                logger.LogError(e, "Database check detected issue(s)");
                var lifetime = serviceScope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
                lifetime.StopApplication();
            }

            intervalTrigger.Advance(dateTime);
        }

    }
}

