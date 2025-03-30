using System;
using System.Collections.Generic;

namespace PowerView.Model.Repository
{
    public interface ISeriesNameRepository
    {
        IList<SeriesName> GetSeriesNames();
    }
}

