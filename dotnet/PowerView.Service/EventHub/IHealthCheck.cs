using System;

namespace PowerView.Service.EventHub
{
  public interface IHealthCheck
  {
    void DailyCheck(DateTime dateTime);
  }
}

