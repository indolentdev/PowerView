using System;
using System.Collections.Generic;

namespace PowerView.Model.Repository
{
    public interface IGaugeRepository
    {
        ICollection<GaugeValueSet> GetLatest(DateTime dateTime);
        ICollection<GaugeValueSet> GetCustom(DateTime dateTime);
    }
}
