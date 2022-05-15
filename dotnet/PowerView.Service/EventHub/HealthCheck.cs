using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
  internal class HealthCheck : IHealthCheck
  {
    private readonly ILogger logger;
    private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(1);

    private readonly IIntervalTrigger intervalTrigger;
    private readonly IFactory factory;

    public HealthCheck(ILogger<HealthCheck> logger, IIntervalTrigger intervalTrigger, IFactory factory)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.intervalTrigger = intervalTrigger ?? throw new ArgumentNullException(nameof(intervalTrigger));
      this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

      this.intervalTrigger.Setup(new TimeSpan(0, 15, 0), TimeSpan.FromDays(1));
    }

    public void DailyCheck(DateTime dateTime)
    {
      if (!intervalTrigger.IsTriggerTime(dateTime))
      {
        return;
      }
        
      using (var ownedDbCheck = factory.Create<IDbCheck>())
      {
        logger.LogInformation("Performing database check");
        try
        {
          ownedDbCheck.Value.CheckDatabase();
          logger.LogInformation("Database check completed");
        }
        catch (DataStoreCorruptException e)
        {
          logger.LogError(e, "Database check detected issue(s)");
          using (var ownedExitSignalProvider = factory.Create<IExitSignalProvider>())
          {
             ownedExitSignalProvider.Value.FireExitEvent();
          }
        }

      }

      intervalTrigger.Advance(dateTime);
    }

  }
}

