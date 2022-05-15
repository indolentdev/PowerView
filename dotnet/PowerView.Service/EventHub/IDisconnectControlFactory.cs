using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
  internal interface IDisconnectControlFactory
  {
    void Process(IList<LiveReading> liveReadings);
  }
}
