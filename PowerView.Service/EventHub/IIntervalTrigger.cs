using System;
namespace PowerView.Service.EventHub
{
  public interface IIntervalTrigger
  {
    void Setup(TimeSpan timeOfDayAtTimezone, TimeSpan interval);
    bool IsTriggerTime(DateTime dateTime);
    void Advance(DateTime dateTime);
  }
}
