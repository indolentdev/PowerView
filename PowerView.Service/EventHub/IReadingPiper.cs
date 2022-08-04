using System;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub
{
  public interface IReadingPiper
  {
    void PipeLiveReadings(IServiceScope serviceScope, DateTime dateTime);
    void PipeDayReadings(IServiceScope serviceScope, DateTime dateTime);
    void PipeMonthReadings(IServiceScope serviceScope, DateTime dateTime);
  }
}

