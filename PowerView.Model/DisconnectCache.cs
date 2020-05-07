using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class DisconnectCache : IDisconnectCache
  {
    private readonly Dictionary<ISeriesName, IDisconnectCacheItem> cacheItems;

    public DisconnectCache()
    {
      cacheItems = new Dictionary<ISeriesName, IDisconnectCacheItem>(4);
    }

    public int Count { get { return cacheItems.Count; } }

    public void SynchronizeRules(ICollection<IDisconnectRule> rules)
    {
      SynchronizeRules(rules, rule => new DisconnectCacheItem(rule));
    }

    internal void SynchronizeRules(ICollection<IDisconnectRule> rules, Func<IDisconnectRule, IDisconnectCacheItem> cacheItemFactory)
    {
      if (rules == null) throw new ArgumentNullException("rules");

      var obsoleteKeys = cacheItems.Keys.Except(rules.Select(x => x.EvaluationName)).ToList();
      foreach (var key in obsoleteKeys)
      {
        cacheItems.Remove(key);
      }
      foreach (var rule in rules)
      {
        if (cacheItems.ContainsKey(rule.EvaluationName))
        {
          continue;
        }
        cacheItems.Add(rule.EvaluationName, cacheItemFactory(rule));
      }
    }

    public void Add(ICollection<LiveReading> liveReadings)
    {
      var itemGropups = liveReadings.SelectMany(lr => lr.GetRegisterValues(), (reading, register) => 
                                      new { Key = new SeriesName(reading.Label, register.ObisCode), reading, register })
                          .GroupBy(x => x.Key, x => x);
      
      foreach (var itemGroup in itemGropups)
      { 
        if (!cacheItems.ContainsKey(itemGroup.Key))
        {
          continue;
        }
        cacheItems[itemGroup.Key].Add(itemGroup.Select(x => new TimeRegisterValue(x.reading.DeviceId, x.reading.Timestamp,
                                                           x.register.Value, x.register.Scale, x.register.Unit)));
      }
    }

    public void Calculate(DateTime time)
    {
      foreach (var cacheItem in cacheItems)
      {
        cacheItem.Value.Calculate(time);
      }
    }

    public IDictionary<ISeriesName, bool> GetStatus()
    {
      return cacheItems.Select(x => new { Name = new SeriesName(x.Value.Rule.Name.Label, x.Value.Rule.Name.ObisCode), x.Value.Connected })
                       .ToDictionary(x => (ISeriesName)x.Name, x => x.Connected);
    }

  }
}
