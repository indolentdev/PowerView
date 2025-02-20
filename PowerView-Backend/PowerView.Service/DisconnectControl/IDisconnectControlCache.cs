using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.DisconnectControl
{
  public interface IDisconnectControlCache
  {
    IDictionary<ISeriesName, bool> GetOutputStatus(string label);
  }
}
