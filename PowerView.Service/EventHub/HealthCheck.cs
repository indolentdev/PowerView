using System;
using System.Reflection;
using log4net;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
  internal class HealthCheck : IHealthCheck
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(1);

    private readonly IIntervalTrigger intervalTrigger;
    private readonly IFactory factory;

    public HealthCheck(IIntervalTrigger intervalTrigger, IFactory factory)
    {
      if (intervalTrigger == null) throw new ArgumentNullException("intervalTrigger");
      if (factory == null) throw new ArgumentNullException("factory");

      this.intervalTrigger = intervalTrigger;
      this.factory = factory;

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
        log.InfoFormat("Performing database check");
        try
        {
          ownedDbCheck.Value.CheckDatabase();
        }
        catch (DataStoreCorruptException e)
        {
          log.Error("Database check detected issue(s)", e);
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

