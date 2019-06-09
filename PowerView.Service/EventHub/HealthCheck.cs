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

    private readonly IFactory factory;

    private DateTime lastRunDay;

    public HealthCheck(IFactory factory)
      : this(factory, DateTime.Now)
    {
    }

    internal HealthCheck(IFactory factory, DateTime dateTime)
    {
      if (factory == null) throw new ArgumentNullException("factory");
      if (dateTime.Kind != DateTimeKind.Local) throw new ArgumentOutOfRangeException("dateTime");

      this.factory = factory;

      lastRunDay = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 15, 0, 0, dateTime.Kind);
    }

    public void DailyCheck(DateTime dateTime)
    {
      if (dateTime < lastRunDay + minimumDayInterval)
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

      lastRunDay = GetDay(dateTime, lastRunDay);
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

