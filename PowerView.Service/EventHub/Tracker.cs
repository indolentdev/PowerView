using System;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
  internal class Tracker : ITracker
  {
    private readonly IIntervalTrigger intervalTrigger;
    private readonly IFactory factory;

    public Tracker(IIntervalTrigger intervalTrigger, IFactory factory)
    {
      if (intervalTrigger == null) throw new ArgumentNullException("intervalTrigger");
      if (factory == null) throw new ArgumentNullException("factory");

      this.intervalTrigger = intervalTrigger;
      this.factory = factory;

      this.intervalTrigger.Setup(new TimeSpan(12, 0, 0), TimeSpan.FromDays(1));
    }

    public void Track(DateTime dateTime)
    {
      if (!intervalTrigger.IsTriggerTime(dateTime))
      {
        return;
      }
      intervalTrigger.Advance(dateTime);

      string sqliteVersion = null;
      using (var envRepository = factory.Create<IEnvironmentRepository>())
      {
        sqliteVersion = envRepository.Value.GetSqliteVersion();
      }

      var monoRuntimeVersion = EnvironmentHelper.GetMonoRuntimeVersion(); 

      using (var usageMonitor = factory.Create<IUsageMonitor>())
      {
        usageMonitor.Value.TrackDing(sqliteVersion, monoRuntimeVersion);
      }
    }

  }
}
