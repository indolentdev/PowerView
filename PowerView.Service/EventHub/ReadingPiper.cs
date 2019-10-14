using System;
using System.Reflection;
using PowerView.Model.Repository;
using log4net;

namespace PowerView.Service.EventHub
{
  internal class ReadingPiper : IReadingPiper
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IIntervalTrigger dayTrigger;
    private readonly IIntervalTrigger monthTrigger;
    private readonly IIntervalTrigger yearTrigger;
    private readonly IFactory factory;

    private bool monthRunAllowed;
    private bool yearRunAllowed;

    public ReadingPiper(IIntervalTrigger dayTrigger, IIntervalTrigger monthTrigger, IIntervalTrigger yearTrigger, IFactory factory)
    {
      if (dayTrigger == null) throw new ArgumentNullException("dayTrigger");
      if (monthTrigger == null) throw new ArgumentNullException("monthTrigger");
      if (yearTrigger == null) throw new ArgumentNullException("yearTrigger");
      if (factory == null) throw new ArgumentNullException("factory");

      this.dayTrigger = dayTrigger;
      this.monthTrigger = monthTrigger;
      this.yearTrigger = yearTrigger;
      this.factory = factory;

      var triggerTimeOfDay = new TimeSpan(0, 45, 0);
      var dayInterval = TimeSpan.FromDays(1);
      this.dayTrigger.Setup(triggerTimeOfDay, dayInterval);
      this.monthTrigger.Setup(triggerTimeOfDay, dayInterval);
      this.yearTrigger.Setup(triggerTimeOfDay, dayInterval);
    }

    #region IReadingPiper implementation

    public void PipeLiveReadings(DateTime dateTime)
    {
      monthRunAllowed = false; // ensure day to month piping only takes place when live to day is "at HEAD"

      if (!dayTrigger.IsTriggerTime(dateTime))
      {
        return;
      }

      log.Info("Piping live readings to day readings");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        for (var i = 0; i < 10; i++)
        {
          var pipedSomething = ownedRepo.Value.PipeLiveReadingsToDayReadings(dateTime);
          if (!pipedSomething)
          {
            monthRunAllowed = true; // live to day piping is complete for now. Allow day to month piping.
            break;
          }
        }
      }

      dayTrigger.Advance(dateTime);
    }

    public void PipeDayReadings(DateTime dateTime)
    {
      yearRunAllowed = false; // ensure month to year piping only takes place when day to month is "at HEAD"

      if (!monthRunAllowed)
      {
        return;
      }

      if (!monthTrigger.IsTriggerTime(dateTime))
      {
        return;
      }

      log.Info("Piping day readings to month readings - if any");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        for (var i = 0; i < 2; i++)
        {
          var pipedSomething = ownedRepo.Value.PipeDayReadingsToMonthReadings(dateTime);
          if (!pipedSomething)
          {
            yearRunAllowed = true; // day to month piping is complete for now. Allow month to year piping.
            break;
          }
        }
      }

      monthTrigger.Advance(dateTime);
    }

    public void PipeMonthReadings(DateTime dateTime)
    {
      if (!yearRunAllowed)
      {
        return;
      }

      if (!yearTrigger.IsTriggerTime(dateTime))
      {
        return;
      }

      log.Info("Piping month readings to year readings - if any");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        ownedRepo.Value.PipeMonthReadingsToYearReadings(dateTime);
      }

      yearTrigger.Advance(dateTime);
    }

    #endregion

  }
}

