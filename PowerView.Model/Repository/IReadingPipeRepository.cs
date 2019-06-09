using System;

namespace PowerView.Model.Repository
{
  public interface IReadingPipeRepository
  {
    bool PipeLiveReadingsToDayReadings(DateTime maximumDateTime);

    void PipeDayReadingsToMonthReadings(DateTime maximumDateTime);

    void PipeMonthReadingsToYearReadings(DateTime maximumDateTime);
  }
}

