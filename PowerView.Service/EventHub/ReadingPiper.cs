using System;
using System.Reflection;
using PowerView.Model.Repository;
using log4net;

namespace PowerView.Service.EventHub
{
  internal class ReadingPiper : IReadingPiper
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(1);

    private readonly IFactory factory;

    private DateTime lastRunDay;

    private bool monthRunAllowed;
    private DateTime lastRunMonth;

    private bool yearRunAllowed;
    private DateTime lastRunYear;

    public ReadingPiper(IFactory factory)
      : this(factory, DateTime.Now)
    {
    }

    internal ReadingPiper(IFactory factory, DateTime dateTime)
    {
      if (factory == null) throw new ArgumentNullException("factory");
      if (dateTime.Kind != DateTimeKind.Local) throw new ArgumentOutOfRangeException("dateTime");

      this.factory = factory;

      lastRunDay = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 45, 0, 0, dateTime.Kind);
      lastRunMonth = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 45, 0, 0, dateTime.Kind);
      lastRunYear = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 45, 0, 0, dateTime.Kind);
    }

    #region IReadingPiper implementation

    public void PipeLiveReadings(DateTime dateTime)
    {
      monthRunAllowed = false; // ensure day to month piping only takes place when live to day is "at HEAD"

      if (dateTime < lastRunDay + minimumDayInterval)
      {
        return;
      }

      log.Info("Piping live readings to day readings");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        for (var i = 0; i < 10; i++)
        {
          var pipedSomething = ownedRepo.Value.PipeLiveReadingsToDayReadings(dateTime.ToUniversalTime());
          if (!pipedSomething)
          {
            monthRunAllowed = true; // live to day piping is complete for now. Allow day to month piping.
            break;
          }
        }
      }

      lastRunDay = GetDay(dateTime, lastRunDay);
    }

    public void PipeDayReadings(DateTime dateTime)
    {
      yearRunAllowed = false; // ensure month to year piping only takes place when day to month is "at HEAD"

      if (!monthRunAllowed)
      {
        return;
      }

      if (dateTime < lastRunMonth + minimumDayInterval)
      {
        return;
      }

      log.Info("Piping day readings to month readings - if any");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        for (var i = 0; i < 2; i++)
        {
          var pipedSomething = ownedRepo.Value.PipeDayReadingsToMonthReadings(dateTime.ToUniversalTime());
          if (!pipedSomething)
          {
            yearRunAllowed = true; // day to month piping is complete for now. Allow month to year piping.
            break;
          }
        }
      }

      lastRunMonth = GetDay(dateTime, lastRunMonth);
    }

    public void PipeMonthReadings(DateTime dateTime)
    {
      if (!yearRunAllowed)
      {
        return;
      }

      if (dateTime < lastRunYear + minimumDayInterval)
      {
        return;
      }

      log.Info("Piping month readings to year readings - if any");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        ownedRepo.Value.PipeMonthReadingsToYearReadings(dateTime.ToUniversalTime());
      }

      lastRunYear = GetDay(dateTime, lastRunYear);
    }

    #endregion

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

