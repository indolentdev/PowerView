using System;

namespace PowerView.Service.EventHub
{
  public interface IReadingPiper
  {
    void PipeLiveReadings(DateTime dateTime);
    void PipeDayReadings(DateTime dateTime);
    void PipeMonthReadings(DateTime dateTime);
  }
}

