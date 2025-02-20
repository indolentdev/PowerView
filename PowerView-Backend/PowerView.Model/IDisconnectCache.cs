using System;
using System.Collections.Generic;

namespace PowerView.Model
{
  public interface IDisconnectCache
  {
    int Count { get; }

    void SynchronizeRules(ICollection<IDisconnectRule> rules);
    void Add(ICollection<Reading> liveReadings);
    void Calculate(DateTime time);
    IDictionary<ISeriesName, bool> GetStatus();
  }
}
