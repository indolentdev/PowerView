using System;
using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  public interface IDisconnectRuleRepository
  {
    void AddDisconnectRule(DisconnectRule disconnectRule);

    void DeleteDisconnectRule(ISeriesName name);

    ICollection<IDisconnectRule> GetDisconnectRules();

    IDictionary<ISeriesName, Unit> GetLatestSerieNames(DateTime dateTime);

  }
}
