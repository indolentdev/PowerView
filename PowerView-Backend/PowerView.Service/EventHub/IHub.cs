using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
    public interface IHub : IDisposable
    {
        void Signal(IList<Reading> liveReadings);
    }
}
