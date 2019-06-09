using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
  internal interface IMqttPublisherFactory
  {
    void Publish(IList<LiveReading> liveReadings);
  }
}
