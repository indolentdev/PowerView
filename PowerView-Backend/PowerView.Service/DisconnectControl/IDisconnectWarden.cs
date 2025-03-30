using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.DisconnectControl
{
    internal interface IDisconnectWarden : IDisposable
    {
        void Process(IList<Reading> liveReadings);
    }
}
