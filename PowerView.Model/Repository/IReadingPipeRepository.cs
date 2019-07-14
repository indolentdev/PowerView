using System;

namespace PowerView.Model.Repository
{
  public interface IReadingPipeRepository
  {
    bool PipeLiveReadingsToDayReadings(DateTime maximumDateTime);

    bool PipeDayReadingsToMonthReadings(DateTime maximumDateTime);

    void PipeMonthReadingsToYearReadings(DateTime maximumDateTime);
  }
}

