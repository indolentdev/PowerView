using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PowerView.Model;
using PowerView.Service.DisconnectControl;

namespace PowerView.Service.EventHub
{
    internal class DisconnectControlFactory : IDisconnectControlFactory
    {

        public void Process(IServiceScope serviceScope, IList<Reading> liveReadings)
        {
            ArgumentNullException.ThrowIfNull(serviceScope);
            ArgumentNullException.ThrowIfNull(liveReadings);
            if (liveReadings.Count == 0)
            {
                return;
            }

            var disconnectWarden = serviceScope.ServiceProvider.GetRequiredService<IDisconnectWarden>();
            disconnectWarden.Process(liveReadings);
        }
    }
}
