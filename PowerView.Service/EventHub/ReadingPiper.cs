using System;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
  internal class ReadingPiper : IReadingPiper
  {
    private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(1);

    private readonly IFactory factory;

    private DateTime lastRunDay;
    private DateTime lastRunMonth;
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
      if (dateTime < lastRunDay + minimumDayInterval)
      {
        return;
      }
        
      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        for (var i = 0; i < 10; i++)
        { 
          var pipedSomething = ownedRepo.Value.PipeLiveReadingsToDayReadings(dateTime.ToUniversalTime());
          if (!pipedSomething)
          {
            break;
          }
        }
      }

      lastRunDay = GetDay(dateTime, lastRunDay);
    }

    public void PipeDayReadings(DateTime dateTime)
    {      
      if (dateTime < lastRunMonth + minimumDayInterval)
      {
        return;
      }

      using (var ownedRepo = factory.Create<IReadingPipeRepository>())
      {
        ownedRepo.Value.PipeDayReadingsToMonthReadings(dateTime.ToUniversalTime());
      }

      lastRunMonth = GetDay(dateTime, lastRunMonth);
    }

    public void PipeMonthReadings(DateTime dateTime)
    {
      if (dateTime < lastRunYear + minimumDayInterval)
      {
        return;
      }

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

