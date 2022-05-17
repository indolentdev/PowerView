using System;
using System.Reflection;
using PowerView.Model.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub
{
  internal class ReadingPiper : IReadingPiper
  {
    private readonly ILogger logger;
    private readonly IIntervalTrigger dayTrigger;
    private readonly IIntervalTrigger monthTrigger;
    private readonly IIntervalTrigger yearTrigger;

    private bool monthRunAllowed;
    private bool yearRunAllowed;

    public ReadingPiper(ILogger<ReadingPiper> logger, IIntervalTrigger dayTrigger, IIntervalTrigger monthTrigger, IIntervalTrigger yearTrigger)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.dayTrigger = dayTrigger ?? throw new ArgumentNullException(nameof(dayTrigger));
      this.monthTrigger = monthTrigger ?? throw new ArgumentNullException(nameof(monthTrigger));
      this.yearTrigger = yearTrigger ?? throw new ArgumentNullException(nameof(yearTrigger));

      var triggerTimeOfDay = new TimeSpan(0, 45, 0);
      var dayInterval = TimeSpan.FromDays(1);
      this.dayTrigger.Setup(triggerTimeOfDay, dayInterval);
      this.monthTrigger.Setup(triggerTimeOfDay, dayInterval);
      this.yearTrigger.Setup(triggerTimeOfDay, dayInterval);
    }

    #region IReadingPiper implementation

    public void PipeLiveReadings(IServiceScope serviceScope, DateTime dateTime)
    {
      monthRunAllowed = false; // ensure day to month piping only takes place when live to day is "at HEAD"

      if (!dayTrigger.IsTriggerTime(dateTime))
      {
        return;
      }

      logger.LogInformation("Piping live readings to day readings");

      var readingPipeRepository = serviceScope.ServiceProvider.GetRequiredService<IReadingPipeRepository>();
      for (var i = 0; i < 50; i++)
      {
        var pipedSomething = readingPipeRepository.PipeLiveReadingsToDayReadings(dateTime);
        if (!pipedSomething)
        {
          monthRunAllowed = true; // live to day piping is "complete" for now (more will be piped later). Allow day to month piping.
          break;
        }
      }

      dayTrigger.Advance(dateTime);
    }

    public void PipeDayReadings(IServiceScope serviceScope, DateTime dateTime)
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

      var readingPipeRepository = serviceScope.ServiceProvider.GetRequiredService<IReadingPipeRepository>();
      for (var i = 0; i < 13; i++)
      {
        var pipedSomething = readingPipeRepository.PipeDayReadingsToMonthReadings(dateTime);
        if (!pipedSomething)
        {
          yearRunAllowed = true; // day to month piping is complete for now. Allow month to year piping.
          break;
        }
      }

      monthTrigger.Advance(dateTime);
    }

    public void PipeMonthReadings(IServiceScope serviceScope, DateTime dateTime)
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

      var readingPipeRepository = serviceScope.ServiceProvider.GetRequiredService<IReadingPipeRepository>();
      readingPipeRepository.PipeMonthReadingsToYearReadings(dateTime);

      yearTrigger.Advance(dateTime);
    }

    #endregion

  }
}

