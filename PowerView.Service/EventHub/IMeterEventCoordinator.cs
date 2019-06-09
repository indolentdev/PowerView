using System;

namespace PowerView.Service.EventHub
{
  public interface IMeterEventCoordinator
  {
    void DetectAndNotify(DateTime dateTime);
  }
}

