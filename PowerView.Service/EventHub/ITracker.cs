using System;

namespace PowerView.Service.EventHub
{
  public interface ITracker
  {
    void Track(DateTime dateTime);
  }
}

