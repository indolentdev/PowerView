using System;

namespace PowerView.Service.EventHub
{
  public interface IMeterEventDetector
  {
    void DetectMeterEvents(DateTime timestamp);
  }
}

