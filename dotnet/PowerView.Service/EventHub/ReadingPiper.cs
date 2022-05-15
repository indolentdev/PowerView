using System;
using System.Reflection;
using PowerView.Model.Repository;
using Microsoft.Extensions.Logging;

namespace PowerView.Service.EventHub
{
  internal class ReadingPiper : IReadingPiper
  {
    private readonly ILogger logger;
    private readonly IIntervalTrigger dayTrigger;
    private readonly IIntervalTrigger monthTrigger;
    private readonly IIntervalTrigger yearTrigger;
    private readonly IFactory factory;

    private bool monthRunAllowed;
    private bool yearRunAllowed;

    public ReadingPiper(ILogger<ReadingPiper> logger, IIntervalTrigger dayTrigger, IIntervalTrigger monthTrigger, IIntervalTrigger yearTrigger, IFactory factory)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.dayTrigger = dayTrigger ?? throw new ArgumentNullException(nameof(dayTrigger));
      this.monthTrigger = monthTrigger ?? throw new ArgumentNullException(nameof(monthTrigger));
      this.yearTrigger = yearTrigger ?? throw new ArgumentNullException(nameof(yearTrigger));
      this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

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

      logger.LogInformation("Piping live readings to day readings");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        for (var i = 0; i < 50; i++)
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

      logger.LogInformation("Piping day readings to month readings - if any");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        for (var i = 0; i < 13; i++)
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

      logger.LogInformation("Piping month readings to year readings - if any");

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        ownedRepo.Value.PipeMonthReadingsToYearReadings(dateTime);
      }

      yearTrigger.Advance(dateTime);
    }

    #endregion

  }
}

