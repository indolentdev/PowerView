using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service
{
    public interface ISeriesNameProvider
    {
        IList<SeriesName> GetSeriesNames();
    }
}

