using System;
using System.Collections.Generic;
using System.Data;

namespace PowerView.Model.Repository
{
    public interface IGeneratorSeriesRepository
    {
        ICollection<GeneratorSeries> GetGeneratorSeries();

        void AddGeneratorSeries(GeneratorSeries generatorSeries);

        void DeleteGeneratorSeries(ISeriesName series);

        ICollection<(SeriesName BaseSeries, DateTime LatestTimestamp)> GetBaseSeries();
    }
}
