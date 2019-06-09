using System;
using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  public interface IDisconnectRuleRepository
  {
    void AddDisconnectRule(DisconnectRule disconnectRule);

    void DeleteDisconnectRule(ISerieName name);

    ICollection<IDisconnectRule> GetDisconnectRules();

    IDictionary<ISerieName, Unit> GetLatestSerieNames(DateTime dateTime);

  }
}
