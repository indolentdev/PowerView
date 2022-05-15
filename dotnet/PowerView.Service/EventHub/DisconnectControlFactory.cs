using System;
using System.Collections.Generic;
using PowerView.Model;
using PowerView.Service.DisconnectControl;

namespace PowerView.Service.EventHub
{
  internal class DisconnectControlFactory :IDisconnectControlFactory
  {
    private readonly IFactory factory;

    public DisconnectControlFactory(IFactory factory)
    {
      if (factory == null) throw new ArgumentNullException("factory");

      this.factory = factory;
    }

    public void Process(IList<LiveReading> liveReadings)
    {
      if (liveReadings == null) throw new ArgumentNullException("liveReadings");
      if (liveReadings.Count == 0)
      {
        return;
      }

      using (var disconnectWarden = factory.Create<IDisconnectWarden>())
      {
        disconnectWarden.Value.Process(liveReadings);
      }
    }
  }
}
