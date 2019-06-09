using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.DisconnectControl
{
  internal interface IDisconnectCalculator
  {
    void SynchronizeAndCalculate(DateTime time, IDisconnectCache disconnectCache, IList<LiveReading> liveReadings);
  }
}
