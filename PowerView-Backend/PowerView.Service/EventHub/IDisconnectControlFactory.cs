using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
  internal interface IDisconnectControlFactory
  {
    void Process(IServiceScope serviceScope, IList<Reading> liveReadings);
  }
}
