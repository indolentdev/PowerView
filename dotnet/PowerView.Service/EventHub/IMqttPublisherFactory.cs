using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
  internal interface IMqttPublisherFactory
  {
    void Publish(IServiceScope serviceScope, IList<LiveReading> liveReadings);
  }
}
