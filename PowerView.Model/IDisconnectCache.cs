using System;
using System.Collections.Generic;

namespace PowerView.Model
{
  public interface IDisconnectCache
  {
    int Count { get; }

    void SynchronizeRules(ICollection<IDisconnectRule> rules);
    void Add(ICollection<LiveReading> liveReadings);
    void Calculate(DateTime time);
    IDictionary<ISerieName, bool> GetStatus();
  }
}
